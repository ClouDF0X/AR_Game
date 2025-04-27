using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;      // ���� ���������� ������ � ����
    [SerializeField] private Button closeIntroButton;    // ������ �Close� �� ��� �����

    private const string FirstRunKey = "has_seen_intro";

    void Start()
    {
        // ����������, �� ��� ������ intro
        bool hasSeen = PlayerPrefs.GetInt(FirstRunKey, 0) == 1;
        if (hasSeen)
        {
            // ���� ������ � ������ ������ � �������� ��� ������
            introPanel.SetActive(false);
            enabled = false;
            return;
        }

        // ������ ������ � �������� ������ � ������ �� ������
        introPanel.SetActive(true);
        closeIntroButton.onClick.AddListener(OnCloseIntro);
    }

    private void OnCloseIntro()
    {
        // ������� ���������, �� ��� ������
        PlayerPrefs.SetInt(FirstRunKey, 1);
        PlayerPrefs.Save();

        // ������ ������ � �������� ������
        introPanel.SetActive(false);
        enabled = false;
    }
}
