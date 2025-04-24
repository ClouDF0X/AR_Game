using System;
using System.Linq;
using System.Collections.Generic;
using Niantic.Experimental.Lightship.AR.WorldPositioning;
using UnityEngine;

public enum PointType { Primary, Secondary }

[Serializable]
public class LatLong
{
    public float latitude;
    public float longitude;
    public PointType pointType;
}

public class PreplaceWorldObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> _possibleObjectsToPlace = new();
    [SerializeField] private List<LatLong> _latLongs = new();
    [SerializeField] private ARWorldPositioningManager _positioningManager;
    [SerializeField] private ARWorldPositioningObjectHelper _objectHelper;
    [Tooltip("Максимальна відстань (в метрах), на якій об’єкт буде видимим")]
    [SerializeField] private float _maxViewDistance = 50f;

    // Тут ми зберігаємо і початкові, і замінені об’єкти
    private readonly List<(GameObject go, LatLong coord)> _placed = new();

    void Start()
    {
        // 1) Прочитати, що вже знайдено
        var foundPrimary = new HashSet<int>(
            PlayerPrefs.GetString("found_primary","")
            .Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
        );
        var foundSecondary = new HashSet<int>(
            PlayerPrefs.GetString("found_secondary","")
            .Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
        );

        // 2) Інстанціюємо всі точки
        for (int i = 0; i < _latLongs.Count; i++)
        {
            var gpsCoord = _latLongs[i];
            GameObject newObject = Instantiate(
                _possibleObjectsToPlace[i % _possibleObjectsToPlace.Count]
            );

            // Зареєструвати в AR-системі
            _objectHelper.AddOrUpdateObject(
                newObject,
                gpsCoord.latitude,
                gpsCoord.longitude,
                0,
                Quaternion.identity
            );

            // Налаштувати ObjectInfo
            var info = newObject.GetComponent<ObjectInfo>();
            if (info != null)
            {
                info.pointType = gpsCoord.pointType;
                info.pointIndex = i;
            }

            // Якщо вже знайдено — одразу приховати й заспавнити замінник
            bool wasFound = (gpsCoord.pointType == PointType.Primary && foundPrimary.Contains(i))
                         || (gpsCoord.pointType == PointType.Secondary && foundSecondary.Contains(i));
            if (wasFound && info != null)
            {
                // Ховаємо початковий
                foreach (var r in newObject.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                foreach (var c in newObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;

                // Спавнимо замінник через ObjectHelper (щоб прив’язався до GPS)
                var repl = Instantiate(info.replacementPrefab);
                _objectHelper.AddOrUpdateObject(
                    repl,
                    gpsCoord.latitude,
                    gpsCoord.longitude,
                    0,
                    Quaternion.identity
                );
                info.isReplaced = true;

                // Зареєструвати замінник у списку, щоб і його ховати за межами дистанції
                _placed.Add((repl, gpsCoord));
            }

            // 3) Додати початковий об’єкт до списку
            _placed.Add((newObject, gpsCoord));
        }

        // Підписка на статус AR (не обов’язково, але корисно для логів)
        _positioningManager.OnStatusChanged += status =>
            Debug.Log("WPS status: " + status);
    }

    void Update()
    {
        if (!_positioningManager.IsAvailable)
            return;

        // Поточні GPS-координати камери
        double camLat = _positioningManager.DefaultCameraHelper.Latitude;
        double camLon = _positioningManager.DefaultCameraHelper.Longitude;

        // Для кожного об’єкта — показати/сховати
        foreach (var (go, coord) in _placed)
        {
            float dist = CalculateDistance(camLat, camLon,
                                           coord.latitude, coord.longitude);
            bool within = dist <= _maxViewDistance;
            if (go.activeSelf != within)
                go.SetActive(within);
        }
    }

    private float CalculateDistance(double lat1, double lon1,
                                    double lat2, double lon2)
    {
        const double R = 6371000;
        double dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Mathf.Deg2Rad * (float)lat1) *
                   Math.Cos(Mathf.Deg2Rad * (float)lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (float)(R * c);
    }

    /// <summary>
    /// Щоб інші скрипти (InteractionUIManager) могли додавати new repl-об’єкти до списку _placed
    /// </summary>
    public void AddPlacedObject(GameObject go, LatLong coord)
    {
        _placed.Add((go, coord));
    }

    // Геттери
    public ARWorldPositioningObjectHelper ObjectHelper => _objectHelper;
    public IReadOnlyList<(GameObject go, LatLong coord)> PlacedObjects => _placed;
    public List<LatLong> GetLatLongs() => _latLongs;
}
