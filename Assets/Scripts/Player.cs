using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public GameObject cardPrefab;
    public List<Card> deck;
    public List<Card> hand;

    /// <summary>
    /// Gets total # of cards player has and instantiates each one with a Card class
    /// </summary>
    public void GetAllCards()
    {
        // 10 is used as an example for how many cards per deck
        for (int i = 0; i < 10; i++)
        {
            GameObject card = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            int id = Random.Range(0, 10);
            card.GetComponent<Card>().id = id;
            card.name = "Card Id: " + id;
            card.GetComponent<Card>().attack.AddRange(new List<int>(){
                Random.Range(1, 4),
                Random.Range(1, 4),
                Random.Range(1, 4),
                Random.Range(1, 4)
            });
            deck.Add(card.GetComponent<Card>());
        }

        Shuffle();
        Draw(3);
    }

    /// <summary>
    /// Shuffle the deck.
    /// </summary>
    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            Card newIndex = deck[randomIndex];
            deck[randomIndex] = deck[i];
            deck[i] = newIndex;
        }
    }

    /// <summary>
    /// Draw x number of cards from deck.
    /// </summary>
    /// <param name="cardsToDraw"># of cards to draw from deck to be added to hand</param>
    public void Draw(int cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            hand.Add(deck[i]);
        }
    }
}
