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
        foreach (var tuple in preplacer.PlacedObjects)
        {
            if (tuple.coord.pointType == PointType.Primary) totalPrimary++;
            else totalSecondary++;
        }
        UpdateUI();
    }

    public void MarkFound(PointType type)
    {
        if (type == PointType.Primary) foundPrimary++;
        else foundSecondary++;

        Debug.Log($"[Progress] Знайдено {foundPrimary}/{totalPrimary} основних; {foundSecondary}/{totalSecondary} другорядних.");
        UpdateUI();
    }

    private void UpdateUI()
    {
        primaryText.text = $"Основні:   {foundPrimary}/{totalPrimary}";
        secondaryText.text = $"Другорядні: {foundSecondary}/{totalSecondary}";
    }
}