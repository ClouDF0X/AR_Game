using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapMarkerPlacer : MonoBehaviour
{
    public RectTransform markerContainer;
    public GameObject markerPrefab;
    public int zoom = 17;
    public int tileSize = 256;
    public int tileRadius = 1;

    public double playerLatitude;
    public double playerLongitude;

    public void SetPlayerCoordinates(double lat, double lon)
    {
        playerLatitude = lat;
        playerLongitude = lon;
    }

    public void UpdateAllMarkers(List<LatLong> worldPositions)
    {
        foreach (Transform child in markerContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"?? Оновлюємо маркери. Отримано {worldPositions.Count} точок.");

        float halfWidth = markerContainer.rect.width / 2f;
        float halfHeight = markerContainer.rect.height / 2f;

        double pTileX, pTileY;
        LatLongToTileDouble(playerLatitude, playerLongitude, zoom, out pTileX, out pTileY);

        Debug.Log($"?? Центр мапи (гравець): lat={playerLatitude}, lon={playerLongitude} ? tileX={pTileX:F2}, tileY={pTileY:F2}");

        int i = 0;
        foreach (LatLong pos in worldPositions)
        {
            double oTileX, oTileY;
            LatLongToTileDouble(pos.latitude, pos.longitude, zoom, out oTileX, out oTileY);

            double deltaTileX = oTileX - pTileX;
            double deltaTileY = oTileY - pTileY;

            float offsetX = (float)(deltaTileX * tileSize);
            float offsetY = (float)(deltaTileY * tileSize);

            float markerPosX = Mathf.Clamp(offsetX, -halfWidth, halfWidth);
            float markerPosY = Mathf.Clamp(-offsetY, -halfHeight, halfHeight);

            GameObject marker = Instantiate(markerPrefab, markerContainer);
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            markerRect.anchoredPosition = new Vector2(markerPosX, markerPosY);

            // Debug.Log($"?? Маркер #{i + 1}: GPS=({pos.latitude}, {pos.longitude}) ? tile=({oTileX:F2}, {oTileY:F2}), offset=({offsetX:F1}, {offsetY:F1}), позиція=({markerPosX:F1}, {markerPosY:F1})");

            i++;
        }

        if (i == 0)
            Debug.LogWarning("?? Жодного маркера не створено.");
    }

    public void LatLongToTileDouble(double lat, double lon, int zoom, out double tileX, out double tileY)
    {
        double latRad = lat * Math.PI / 180;
        int n = 1 << zoom;
        tileX = (lon + 180.0) / 360.0 * n;
        tileY = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n;
    }
}