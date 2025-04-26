using System;                              // для StringSplitOptions
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Niantic.Experimental.Lightship.AR.WorldPositioning;

public class InteractionUIManager : MonoBehaviour
{
    [Header("AR Placement")]
    [SerializeField] private PreplaceWorldObjects _preplacer;
    [SerializeField] private ARWorldPositioningManager _positioningManager;

    [Header("Interact Button")]
    [Tooltip("Prefab вашої кнопки, яку треба показувати над кожним об'єктом")]
    [SerializeField] private Button _interactButtonPrefab;
    [Tooltip("Canvas, куди ми додамо ці кнопки")]
    [SerializeField] private Canvas _uiCanvas;

    [Header("Info Panel")]
    [SerializeField] private RectTransform _infoPanel;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private Button _closeInfoButton;

    [Header("Progress & Summary")]
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private SummaryUIManager _summaryUI;

    [Header("Settings")]
    [SerializeField] private float _showDistance = 5f;

    private Camera _mainCam;
    private GameObject _currentTarget;

    // Мапа AR-об’єкт → його UI-кнопка
    private readonly Dictionary<GameObject, Button> _buttons = new Dictionary<GameObject, Button>();

    void Start()
    {
        _mainCam = Camera.main;
        _infoPanel.gameObject.SetActive(false);
        _closeInfoButton.onClick.AddListener(OnCloseInfoClicked);

        // 1) Інстанціюємо кнопку для кожного PlacedObject
        foreach (var entry in _preplacer.PlacedObjects)
        {
            var go = entry.go;
            // Копіюємо префаб під Canvas
            var btn = Instantiate(_interactButtonPrefab, _uiCanvas.transform);
            btn.gameObject.SetActive(false);
            // Коли натиснуть цю кнопку — викликаємо збережений GameObject
            btn.onClick.AddListener(() => OnInteractButton(go));
            _buttons[go] = btn;
        }
    }

    void Update()
    {
        if (!_positioningManager.IsAvailable) return;

        // 2) Для кожного PlacedObject оновлюємо видимість/позицію кнопки
        foreach (var entry in _preplacer.PlacedObjects)
        {
            var go = entry.go;
            if (go == null || !_buttons.ContainsKey(go))
                continue;

            var info = go.GetComponent<ObjectInfo>();
            bool shouldShow = go.activeSelf
                              && info != null
                              && !info.isReplaced
                              && Vector3.Distance(_mainCam.transform.position, go.transform.position) <= _showDistance;

            var btn = _buttons[go];
            if (shouldShow)
            {
                btn.gameObject.SetActive(true);
                // конвертуємо позицію світу → екран
                Vector3 screenPos = _mainCam.WorldToScreenPoint(go.transform.position);
                btn.transform.position = screenPos + Vector3.up * 50f;
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    // Викликається по натисненню кнопки конкретного об'єкта
    private void OnInteractButton(GameObject go)
    {
        _currentTarget = go;
        var info = go.GetComponent<ObjectInfo>();
        if (info == null) return;

        _infoText.text = info.description;
        _infoPanel.gameObject.SetActive(true);
    }

    private void OnCloseInfoClicked()
    {
        if (_currentTarget == null) return;
        var info = _currentTarget.GetComponent<ObjectInfo>();
        if (info == null || info.isReplaced) return;

        // ————— Спавнимо replacement на GPS —————
        var entry = _preplacer.PlacedObjects.First(e => e.go == _currentTarget);
        var latLong = entry.coord;
        GameObject repl = Instantiate(info.replacementPrefab);
        _preplacer.ObjectHelper.AddOrUpdateObject(
            repl,
            latLong.latitude,
            latLong.longitude,
            0,
            Quaternion.identity
        );

        // ————— Ховаємо оригінал —————
        foreach (var r in _currentTarget.GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in _currentTarget.GetComponentsInChildren<Collider>()) c.enabled = false;
        info.isReplaced = true;

        // ————— Зберігаємо прогрес —————
        _progressManager.MarkFound(info.pointType);
        SaveFoundPoint(info.pointType, info.pointIndex);

        // ————— Додаємо до summary_text —————
        const string summaryKey = "summary_text";
        string prev = PlayerPrefs.GetString(summaryKey, "");
        string updated = string.IsNullOrEmpty(prev)
            ? info.description
            : prev + "\n\n" + info.description;
        PlayerPrefs.SetString(summaryKey, updated);
        PlayerPrefs.Save();
        _summaryUI.NotifySummaryAvailable();

        // ————— Закриваємо панель Info —————
        _infoPanel.gameObject.SetActive(false);
    }

    private void SaveFoundPoint(PointType type, int index)
    {
        string key = type == PointType.Primary ? "found_primary" : "found_secondary";
        string existing = PlayerPrefs.GetString(key, "");
        var list = new List<string>(
            existing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        );
        if (!list.Contains(index.ToString()))
        {
            list.Add(index.ToString());
            PlayerPrefs.SetString(key, string.Join(",", list));
            PlayerPrefs.Save();
        }
    }
}
