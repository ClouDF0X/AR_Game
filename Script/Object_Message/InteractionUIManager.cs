using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Niantic.Experimental.Lightship.AR.WorldPositioning;

public class InteractionUIManager : MonoBehaviour
{
    [SerializeField] private PreplaceWorldObjects _preplacer;
    [SerializeField] private ARWorldPositioningManager _positioningManager;

    [SerializeField] private Button _interactButton;
    [SerializeField] private RectTransform _infoPanel;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private Button _closeInfoButton;

    [Header("Progress")]
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
        foreach (var tuple in _preplacer.PlacedObjects)
        {
            var go = tuple.go;
            if (go == null || !go.activeSelf) continue;

            var info = go.GetComponent<ObjectInfo>();
            if (info != null && info.isReplaced) continue;

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
        if (info != null)
        {
            _infoText.text = info.description;
            _infoPanel.gameObject.SetActive(true);
            _interactButton.gameObject.SetActive(false);
        }
    }

    private void OnCloseInfoClicked()
    {
        if (_currentTarget == null) return;
        var info = _currentTarget.GetComponent<ObjectInfo>();
        if (info != null && !info.isReplaced)
        {
            if (info.replacementPrefab != null)
                Instantiate(info.replacementPrefab, _currentTarget.transform.position, _currentTarget.transform.rotation);

            info.isReplaced = true;
            foreach (var r in _currentTarget.GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var c in _currentTarget.GetComponentsInChildren<Collider>()) c.enabled = false;

            _progressManager.MarkFound(info.pointType);
        }

        _infoPanel.gameObject.SetActive(false);
        // кнопка ховається в Update(), бо panelOpen == false
    }
}