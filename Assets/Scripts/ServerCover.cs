using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ServerCover : MonoBehaviour {

    public Animator dancingSquares;

    public void OnEnable()
    {
        dancingSquares.SetBool("showSquares", true);
    }

    public void EnableAnimations()
    {
        dancingSquares.SetBool("showSquares", true);
    }

    public void DisableAnimations()
    {
        dancingSquares.SetBool("showSquares", false);
    }
}