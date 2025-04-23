using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Niantic.Experimental.Lightship.AR.WorldPositioning;

public class StaticMapLoader : MonoBehaviour
{
    [SerializeField] private RawImage miniMapImage;
    [SerializeField] private int zoom = 17;
    [SerializeField] private int tileSize = 256;
    [SerializeField] private int tileRadius = 1;
    [SerializeField] private float updateInterval = 5f;

    [SerializeField] private ARWorldPositioningManager _positioningManager;
    [SerializeField] private MiniMapMarkerPlacer _miniMapMarkerPlacer;
    [SerializeField] private PreplaceWorldObjects _preplaceWorldObjects;

    private double lastLat = 0;
    private double lastLon = 0;
    private bool isFirstUpdate = true;

    void Start()
    {
        Debug.Log("Старт локаційного сервісу...");
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("GPS вимкнений користувачем");
            yield break;
        }

        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait-- > 0)
        {
            Debug.Log("Очікування запуску GPS...");
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError("Сервіс локації не запущено");
            yield break;
        }

        Debug.Log("GPS активовано. Починаємо оновлення мінікарти...");
        StartCoroutine(UpdateMiniMapPeriodically());
    }

    IEnumerator UpdateMiniMapPeriodically()
    {
        while (true)
        {
            double lat, lon;
            if (_positioningManager != null && _positioningManager.IsAvailable)
            {
                lat = _positioningManager.DefaultCameraHelper.Latitude;
                lon = _positioningManager.DefaultCameraHelper.Longitude;
            }
            else
            {
                lat = Input.location.lastData.latitude;
                lon = Input.location.lastData.longitude;
            }

            Debug.Log($"[MapLoader] Using coords: lat={lat}, lon={lon} (WPS ready? {(_positioningManager?.IsAvailable ?? false)})");

            if (isFirstUpdate ||
                Math.Abs(lat - lastLat) > 0.0001 ||
                Math.Abs(lon - lastLon) > 0.0001)
            {
                StartCoroutine(DownloadAndSetMap(lat, lon));
                lastLat = lat;
                lastLon = lon;
                isFirstUpdate = false;
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    IEnumerator DownloadAndSetMap(double lat, double lon)
    {
        LatLongToTileDouble(lat, lon, zoom, out double tileX, out double tileY);
        int centerTileX = (int)Math.Floor(tileX);
        int centerTileY = (int)Math.Floor(tileY);

        int tileCount = tileRadius * 2 + 1;
        int fullSize = tileSize * tileCount;
        var fullTexture = new Texture2D(fullSize, fullSize);

        for (int y = -tileRadius; y <= tileRadius; y++)
            for (int x = -tileRadius; x <= tileRadius; x++)
            {
                string url = $"https://tile.openstreetmap.org/{zoom}/{centerTileX + x}/{centerTileY + y}.png";
                using (WWW www = new WWW(url))
                {
                    yield return www;
                    if (string.IsNullOrEmpty(www.error) && www.texture)
                    {
                        fullTexture.SetPixels(
                            (x + tileRadius) * tileSize,
                            (tileRadius - y) * tileSize,
                            tileSize,
                            tileSize,
                            www.texture.GetPixels());
                    }
                    else
                    {
                        Debug.LogWarning($"❌ Не вдалося завантажити тайл: {url}, error: {www.error}");
                    }
                }
            }
        fullTexture.Apply();

        float offsetX = (float)((tileX - centerTileX) * tileSize);
        float offsetY = (float)((tileY - centerTileY) * tileSize);

        int cropSize = tileSize * tileRadius * 2;
        int centerPixelX = tileSize * tileRadius;
        int centerPixelY = tileSize * tileRadius;

        float dx = Mathf.Abs(offsetX) < 1f ? 0f : offsetX;
        float dy = Mathf.Abs(offsetY) < 1f ? 0f : offsetY;

        int startX = Mathf.Clamp(
            (int)(centerPixelX - cropSize / 2 + dx),
            0,
            fullTexture.width - cropSize);
        int startY = Mathf.Clamp(
            (int)(centerPixelY - cropSize / 2 + dy),
            0,
            fullTexture.height - cropSize);

        var croppedTexture = new Texture2D(cropSize, cropSize);
        var pixels = fullTexture.GetPixels(startX, startY, cropSize, cropSize);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        miniMapImage.texture = croppedTexture;
        Debug.Log("✅ Мінікарта оновлена з центром точно по GPS.");

        var gpsPoints = _preplaceWorldObjects.GetLatLongs();
        _miniMapMarkerPlacer.SetPlayerCoordinates(lat, lon);
        _miniMapMarkerPlacer.UpdateAllMarkers(gpsPoints);
    }

    public void ForceUpdateMap()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("Cannot force-update map: Location service is not running");
            return;
        }
        isFirstUpdate = true;
        double lat = (_positioningManager != null && _positioningManager.IsAvailable)
            ? _positioningManager.DefaultCameraHelper.Latitude
            : Input.location.lastData.latitude;
        double lon = (_positioningManager != null && _positioningManager.IsAvailable)
            ? _positioningManager.DefaultCameraHelper.Longitude
            : Input.location.lastData.longitude;
        StartCoroutine(DownloadAndSetMap(lat, lon));
        Debug.Log("📍 ForceUpdateMap: примусове оновлення мінікарти запущено");
    }

    void LatLongToTileDouble(double lat, double lon, int zoom, out double tileX, out double tileY)
    {
        double latRad = lat * Math.PI / 180;
        int n = 1 << zoom;
        tileX = (lon + 180.0) / 360.0 * n;
        tileY = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n;
    }
}