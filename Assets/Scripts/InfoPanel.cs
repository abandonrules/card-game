using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InfoPanel : MonoBehaviour 
{
    public string action;
    public string playerName;
    public string transferPasscode;

    public void Show()
    {
        LeanTween.value(0f, 1f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                transform.localScale = new Vector2(1f, val);
            });
    }

    public void Hide()
    {
        if (GetComponentInChildren<TMP_InputField>())
        {
            GetComponentInChildren<TMP_InputField>().text = "";
        }
        if (transform.Find("Error Text"))
        {
            transform.Find("Error Text").GetComponent<TextMeshProUGUI>().text = "";
        }

        LeanTween.value(1f, 0f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                transform.localScale = new Vector2(1f, val);
            });
    }

    public void RestartApp()
    {
        SceneManager.LoadScene(0);
    }

    public void RetryAction(DeviceLogin deviceLogin)
    {
        Hide();
        if (action == "Login")
        {
            deviceLogin.LoginToPlayFab(PlayerPrefsManager.GetPlayerCustomId());
        }
        else if (action == "Create")
        {
            deviceLogin.CreatePlayFabAccount(PlayerPrefsManager.GetPlayerCustomId(), playerName);
        }
        else if (action == "Transfer")
        {
            deviceLogin.TransferPlayFabAccount(transferPasscode);
        }
    }
}
