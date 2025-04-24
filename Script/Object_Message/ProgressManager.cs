using System.Linq;
using UnityEngine;
using TMPro;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private PreplaceWorldObjects preplacer;
    [SerializeField] private TextMeshProUGUI primaryText;
    [SerializeField] private TextMeshProUGUI secondaryText;

    private int totalPrimary, totalSecondary;
    private int foundPrimary, foundSecondary;

    void Start()
    {
        // 1) Рахуємо загальні за списком LatLong
        var allCoords = preplacer.GetLatLongs();
        totalPrimary = allCoords.Count(c => c.pointType == PointType.Primary);
        totalSecondary = allCoords.Count(c => c.pointType == PointType.Secondary);

        // 2) Підвантажуємо кількість вже знайдених
        var savedP = PlayerPrefs.GetString("found_primary", "");
        if (!string.IsNullOrEmpty(savedP))
            foundPrimary = savedP.Split(',').Length;

        var savedS = PlayerPrefs.GetString("found_secondary", "");
        if (!string.IsNullOrEmpty(savedS))
            foundSecondary = savedS.Split(',').Length;

        // 3) Оновлюємо UI
        UpdateUI();
    }

    public void MarkFound(PointType type)
    {
        if (type == PointType.Primary) foundPrimary++;
        else foundSecondary++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        primaryText.text = $"Основні:   {foundPrimary}/{totalPrimary}";
        secondaryText.text = $"Другорядні: {foundSecondary}/{totalSecondary}";
    }
}
