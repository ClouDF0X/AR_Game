using System.Collections;
using UnityEngine;
using TMPro;

public class AppInitializer : MonoBehaviour
{
    [Header("UI для повідомлень")]
    public GameObject messagePanel;
    public TMP_Text messageText;

    [Header("Параметри")]
    public float checkInterval = 2f;

    [Header("Map Loader")]
    public StaticMapLoader mapLoader;

    bool _wasUnavailable = true;

    void Start()
    {
        messagePanel.SetActive(false);
        StartCoroutine(CheckServicesLoop());
    }

    IEnumerator CheckServicesLoop()
    {
        while (true)
        {
            bool gpsOn = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
            bool netOn = Application.internetReachability != NetworkReachability.NotReachable;

            if (!gpsOn)
            {
                messageText.text = "Увімкніть GPS у налаштуваннях.";
                messagePanel.SetActive(true);
                _wasUnavailable = true;
            }
            else if (!netOn)
            {
                messageText.text = "Підключіться до Інтернету.";
                messagePanel.SetActive(true);
                _wasUnavailable = true;
            }
            else
            {
                messagePanel.SetActive(false);
                if (_wasUnavailable)
                {
                    mapLoader.ForceUpdateMap();
                    _wasUnavailable = false;
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }
}
