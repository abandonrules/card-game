using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

    // Reference of card's in-game id
    public int id;
    // Reference of card's owner
    public Transform owner;
    // Reference of card's sprite
    public Sprite sprite;
    // Reference of card's attack; KEYS: "top", "right", "bottom", "left"
    public Dictionary<string, int> attack = new Dictionary<string, int>();
    // Reference of card's effect (if it has one)
    //public string effect;
    public List<Text> attackUI;

    public void SetOwner(Transform newOwner)
    {
        if (owner != newOwner)
        {
            owner = newOwner;
        }
    }

    public void ShowAttackUI()
    {
        foreach(Text val in attackUI)
        {
            val.color = Color.black;
        }
    }

    public void HideAttackUI()
    {
        foreach (Text val in attackUI)
        {
            val.color = Color.clear;
        }
    }
}
