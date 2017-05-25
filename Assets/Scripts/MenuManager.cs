using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour 
{
    private bool startGame;

    [SerializeField]
    private string debugCustomId;

    [SerializeField]
    private Image title;
    [SerializeField]
    private Button startButton;

    public DeviceLogin deviceLogin;
    public ColorPanel colorPanel;
    public ServerCover serverCover;

    public Transform cover;

    public List<InfoPanel> infoPanels;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
        }
        if (!Application.isShowingSplashScreen && !startGame)
        {
            StartCoroutine(SceneTransition.FadeOut(Color.black, 1f, 0f, FadeInTitleUI));
            startGame = true;
        }
    }

    private void FadeInTitleUI()
    {
        startButton.interactable = true;

        LeanTween.moveLocalY(cover.gameObject, -8.5f, 1.5f)
            .setEase(LeanTweenType.easeInOutBack)
            .setOnComplete(() =>
            {
                LeanTween.value(title.gameObject, 2f, 1f, 1.5f)
                    .setEase(LeanTweenType.easeOutElastic)
                    .setOnUpdate((float val) =>
                    {
                        title.transform.localScale = new Vector2(val, val);
                    })
                    .setOnComplete(() =>
                    {
                        LeanTween.value(startButton.gameObject, -400f, -200f, 1f)
                            .setEase(LeanTweenType.easeOutBack)
                            .setOnUpdate((float val) =>
                            {
                                startButton.transform.localPosition = new Vector2(0, val);
                            });

                        LeanTween.value(startButton.gameObject, 0f, 1f, 1f)
                            .setEase(LeanTweenType.easeInQuad)
                            .setOnUpdate((float val) =>
                            {
                                startButton.GetComponent<Image>().color = new Color(startButton.GetComponent<Image>().color.r, startButton.GetComponent<Image>().color.g, startButton.GetComponent<Image>().color.b, val);
                            });
                    });

                LeanTween.value(0f, 1f, 0.2f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnUpdate((float val) =>
                    {
                        title.color = new Color(title.color.r, title.color.g, title.color.b, val);
                    });
            });
        
    }

    private void FadeOutTitleUI()
    {

    }

    public void ShowPanel(InfoPanel panel)
    {
        panel.Show();
    }

    public void HidePanel(InfoPanel panel)
    {
        panel.Hide();
    }

    public void OnClickLoginToPlayFab()
    {
        if (string.IsNullOrEmpty(debugCustomId))
        {
            string customId = PlayerPrefsManager.GetPlayerCustomId();
            if (string.IsNullOrEmpty(customId))
            {
                ShowPanel(infoPanels[0]);
            }
            else
            {
                deviceLogin.LoginToPlayFab(customId);   
            }
        }
        else
        {
            deviceLogin.LoginToPlayFab(debugCustomId);
        }
    }

    public void OnClickCreateAccount(TMP_InputField newPlayerName)
    {
        if (string.IsNullOrEmpty(newPlayerName.text))
        {
            TextMeshProUGUI messageText = newPlayerName.transform.parent.Find("Error Text").GetComponent<TextMeshProUGUI>();
            messageText.text = "Field is empty";
            return;
        }

        string newCustomId = PlayerPrefsManager.GetRandomCustomId();
        PlayerPrefsManager.SetPlayerCustomId(newCustomId);
        deviceLogin.CreatePlayFabAccount(newCustomId, newPlayerName.text);
    }

    public void OnClickTransferAccount(TMP_InputField customId)
    {
        if (string.IsNullOrEmpty(customId.text))
        {
            TextMeshProUGUI messageText = customId.transform.parent.Find("Error Text").GetComponent<TextMeshProUGUI>();
            messageText.text = "Field is empty";
            return;
        }

        deviceLogin.TransferPlayFabAccount(customId.text);
    }

    public void OnClickRetryLogin()
    {
        string customId = PlayerPrefsManager.GetPlayerCustomId();
        deviceLogin.LoginToPlayFab(customId);
    }
}
