using System;
using System.Collections.Generic;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using Niantic.Experimental.Lightship.AR.XRSubsystems;
using UnityEngine;

public enum PointType { Primary, Secondary }

[Serializable]
public class LatLong
{
    public float latitude;
    public float longitude;
    public PointType pointType;
}

/// <summary>
/// Відповідає за попереднє розміщення AR-об'єктів за GPS-координатами
/// </summary>
public class PreplaceWorldObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> _possibleObjectsToPlace = new();
    [SerializeField] private List<LatLong> _latLongs = new();
    [SerializeField] private ARWorldPositioningManager _positioningManager;
    [SerializeField] private ARWorldPositioningObjectHelper _objectHelper;

    [Tooltip("Максимальна відстань (в метрах), на якій об’єкт буде видимим")]
    [SerializeField] private float _maxViewDistance = 50f;

    private readonly List<(GameObject go, LatLong coord)> _placed = new();

    void Start()
    {
        for (int i = 0; i < _latLongs.Count; i++)
        {
            var gpsCoord = _latLongs[i];
            GameObject newObject = Instantiate(
                _possibleObjectsToPlace[i % _possibleObjectsToPlace.Count]);

            _objectHelper.AddOrUpdateObject(
                newObject,
                gpsCoord.latitude,
                gpsCoord.longitude,
                0,
                Quaternion.identity);

            // Передаємо тип до компонента ObjectInfo:
            var info = newObject.GetComponent<ObjectInfo>();
            if (info != null)
                info.pointType = gpsCoord.pointType;

            Debug.Log($"Added {newObject.name} with latitude {gpsCoord.latitude} and longitude {gpsCoord.longitude}");
            _placed.Add((newObject, gpsCoord));
        }

        _positioningManager.OnStatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(WorldPositioningStatus status)
    {
        Debug.Log("Status changed to " + status);
    }

    void Update()
    {
        if (!_positioningManager.IsAvailable)
            return;

        double camLat = _positioningManager.DefaultCameraHelper.Latitude;
        double camLon = _positioningManager.DefaultCameraHelper.Longitude;

        foreach (var (go, coord) in _placed)
        {
            float dist = CalculateDistance(camLat, camLon, coord.latitude, coord.longitude);
            bool within = dist <= _maxViewDistance;
            if (go.activeSelf != within)
                go.SetActive(within);
        }
    }

    private float CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        double dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Mathf.Deg2Rad * (float)lat1) * Math.Cos(Mathf.Deg2Rad * (float)lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (float)(R * c);
    }

    // Ось ця властивість повертає список розміщених об’єктів
    public IReadOnlyList<(GameObject go, LatLong coord)> PlacedObjects => _placed;

    public List<LatLong> GetLatLongs() => _latLongs;
}