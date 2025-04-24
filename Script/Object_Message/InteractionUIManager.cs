using System.Linq;                                // <-- потрібен для First()
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Niantic.Experimental.Lightship.AR.WorldPositioning;

public class InteractionUIManager : MonoBehaviour
{
    [SerializeField] private PreplaceWorldObjects _preplacer;
    [SerializeField] private ARWorldPositioningObjectHelper _objectHelper;  // <-- нове поле
    [SerializeField] private ARWorldPositioningManager _positioningManager;
    [SerializeField] private Button _interactButton;
    [SerializeField] private RectTransform _infoPanel;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private Button _closeInfoButton;
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private float _showDistance = 5f;

    private Camera _mainCam;
    private GameObject _currentTarget;

    void Start()
    {
        _mainCam = Camera.main;
        _interactButton.gameObject.SetActive(false);
        _infoPanel.gameObject.SetActive(false);
        _interactButton.onClick.AddListener(OnInteractClicked);
        _closeInfoButton.onClick.AddListener(OnCloseInfoClicked);
    }

    void Update()
    {
        if (!_positioningManager.IsAvailable) return;
        _currentTarget = null;

        foreach (var (go, coord) in _preplacer.PlacedObjects)
        {
            if (go == null || !go.activeSelf) continue;
            var info = go.GetComponent<ObjectInfo>();
            if (info == null || info.isReplaced) continue;
            if (Vector3.Distance(_mainCam.transform.position, go.transform.position) <= _showDistance)
            {
                _currentTarget = go;
                break;
            }
        }

        bool panelOpen = _infoPanel.gameObject.activeSelf;
        if (_currentTarget != null && !panelOpen)
        {
            _interactButton.gameObject.SetActive(true);
            var screenPos = _mainCam.WorldToScreenPoint(_currentTarget.transform.position);
            _interactButton.transform.position = screenPos + Vector3.up * 50f;
        }
        else
        {
            _interactButton.gameObject.SetActive(false);
            if (!panelOpen) _infoPanel.gameObject.SetActive(false);
        }
    }

    private void OnInteractClicked()
    {
        if (_currentTarget == null) return;
        var info = _currentTarget.GetComponent<ObjectInfo>();
        if (info == null) return;
        _infoText.text = info.description;
        _infoPanel.gameObject.SetActive(true);
        _interactButton.gameObject.SetActive(false);
    }

    private void OnCloseInfoClicked()
    {
        if (_currentTarget == null) return;
        var info = _currentTarget.GetComponent<ObjectInfo>();
        if (info == null || info.isReplaced) return;

        // 1) Знайти точку в списку, щоб дістати координати
        var tuple = _preplacer.PlacedObjects
                      .First(t => t.go == _currentTarget);
        var latLong = tuple.coord;

        // 2) Спавнимо замінник через ваш ObjectHelper, щоб він з’явився за GPS
        GameObject repl = Instantiate(info.replacementPrefab);
        _objectHelper.AddOrUpdateObject(
            repl,
            latLong.latitude,
            latLong.longitude,
            0,
            Quaternion.identity
        );

        // 3) Ховаємо оригінал
        foreach (var r in _currentTarget.GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in _currentTarget.GetComponentsInChildren<Collider>()) c.enabled = false;
        info.isReplaced = true;

        // 4) Оновлюємо прогрес та зберігаємо
        _progressManager.MarkFound(info.pointType);
        SaveFoundPoint(info.pointType, info.pointIndex);

        _infoPanel.gameObject.SetActive(false);
    }

    private void SaveFoundPoint(PointType type, int index)
    {
        string key = type == PointType.Primary ? "found_primary" : "found_secondary";
        var existing = PlayerPrefs.GetString(key, "");
        var list = new System.Collections.Generic.List<string>(
            existing.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
        );
        if (!list.Contains(index.ToString()))
        {
            list.Add(index.ToString());
            PlayerPrefs.SetString(key, string.Join(",", list));
            PlayerPrefs.Save();
        }
    }
}
