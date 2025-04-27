using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;      // ¬аша прив≥тальна панель у сцен≥
    [SerializeField] private Button closeIntroButton;    //  нопка УCloseФ на ц≥й панел≥

    private const string FirstRunKey = "has_seen_intro";

    void Start()
    {
        // ѕерев≥р€Їмо, чи вже бачили intro
        bool hasSeen = PlayerPrefs.GetInt(FirstRunKey, 0) == 1;
        if (hasSeen)
        {
            // якщо бачили Ч ховаЇмо панель ≥ вимикаЇмо цей скрипт
            introPanel.SetActive(false);
            enabled = false;
            return;
        }

        // ѕерший запуск Ч показуЇмо панель й чекаЇмо на кнопку
        introPanel.SetActive(true);
        closeIntroButton.onClick.AddListener(OnCloseIntro);
    }

    private void OnCloseIntro()
    {
        // —тавимо прапорець, що вже бачили
        PlayerPrefs.SetInt(FirstRunKey, 1);
        PlayerPrefs.Save();

        // ’оваЇмо панель ≥ вимикаЇмо скрипт
        introPanel.SetActive(false);
        enabled = false;
    }
}
