using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonLogic : MonoBehaviour {

	public void Pinch()
    {
        transform.localScale = new Vector2(0.8f, 0.8f);
    }

    public void Cancel()
    {
        if (Input.GetMouseButton(0))
        {
            LeanTween.value(gameObject, 0.8f, 1f, 0.5f)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnUpdate((float val) =>
                {
                    transform.localScale = new Vector2(val, val);
                });
        }
    }

    public void Pull()
    {
        LeanTween.value(gameObject, 0.8f, 1f, 0.5f)
            .setEase(LeanTweenType.easeOutElastic)
            .setOnUpdate((float val) =>
            {
                transform.localScale = new Vector2(val, val);
            });
    }
}
