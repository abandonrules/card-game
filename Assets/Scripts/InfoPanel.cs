using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InfoPanel : MonoBehaviour 
{
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
}
