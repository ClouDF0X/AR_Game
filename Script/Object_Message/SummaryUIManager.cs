using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _summaryPanel;
    [SerializeField] private Button _summaryButton;
    [SerializeField] private Button _closeSummaryButton;
    [SerializeField] private TextMeshProUGUI _summaryText;
    [SerializeField] private RectTransform _contentRect;

    private const string SummaryKey = "summary_text";

    void Start()
    {
        _summaryPanel.SetActive(false);

        // Покажемо SummaryButton, якщо вже є збережений текст
        bool hasSummary = PlayerPrefs.HasKey("summary_text")
                          && !string.IsNullOrEmpty(PlayerPrefs.GetString("summary_text"));
        _summaryButton.gameObject.SetActive(hasSummary);

        _summaryButton.onClick.AddListener(OpenSummary);
        _closeSummaryButton.onClick.AddListener(() => _summaryPanel.SetActive(false));
    }

    private void OpenSummary()
    {
        string allText = PlayerPrefs.GetString(SummaryKey, "");
        if (string.IsNullOrEmpty(allText))
        {
            Debug.Log("[Summary] Немає тексту для відображення.");
            return;
        }

        _summaryText.text = allText;

        // щоб ScrollView підхопив висоту
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);

        _summaryPanel.SetActive(true);
    }

    public void NotifySummaryAvailable()
    {
        _summaryButton.gameObject.SetActive(true);
    }
}
