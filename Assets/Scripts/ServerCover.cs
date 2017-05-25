using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ServerCover : MonoBehaviour {

    public Animator dancingSquares;
    public TextMeshProUGUI connectingText;

    public void OnEnable()
    {
        dancingSquares.SetBool("showSquares", true);
        dancingSquares.gameObject.SetActive(true);
        connectingText.gameObject.SetActive(true);
    }

    public void EnableAnimations()
    {
        dancingSquares.speed = 1;
        dancingSquares.GetComponent<Image>().enabled = true;
        connectingText.gameObject.SetActive(true);
    }

    public void DisableAnimations()
    {
        dancingSquares.speed = 0;
        dancingSquares.GetComponent<Image>().enabled = false;
        connectingText.gameObject.SetActive(false);
    }
}