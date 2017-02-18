using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

    // Reference of card's in-game id
    public int id;
    // Reference of card's attack[top, right, bottom, left]
	public List<int> attack;
}
