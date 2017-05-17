using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class SceneTransition : MonoBehaviour {

	public static IEnumerator FadeIn(string sceneName, Color color, float time, float delay)
    {
        SceneTransition transition = FindObjectOfType<SceneTransition>();
        transition.GetComponent<Image>().raycastTarget = true;

        LeanTween.value(0, 1, time)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
                transition.GetComponent<Image>().color = new Color(color.r, color.g, color.b, val);
            })
            .setOnComplete(() =>
            {
                LeanTween.delayedCall(delay, () =>
                {
                    SceneManager.LoadScene(sceneName);
                });
            });

        yield return new WaitForSeconds(time + delay);
    }

    public static IEnumerator FadeOut(Color color, float time, float delay, Action methodCallback)
    {
        SceneTransition transition = FindObjectOfType<SceneTransition>();
        transition.GetComponent<Image>().color = color;

        LeanTween.delayedCall(delay, () =>
        {
            LeanTween.value(1, 0, time)
                .setEase(LeanTweenType.easeInQuad)
                .setOnUpdate((float val) =>
                {
                    transition.GetComponent<Image>().color = new Color(color.r, color.g, color.b, val);
                })
                .setOnComplete(() =>
                {
                    transition.GetComponent<Image>().raycastTarget = false;
                    methodCallback();
                });
        });

        yield return new WaitForSeconds(time + delay);
    }
}
