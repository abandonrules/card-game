using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public GameObject cardPrefab;
    public List<Card> deck;
    public List<Card> hand;

    // Get Card data from web server
    // Store card data in temp list
    // Iterate list and store card data in instantiated object
    public void GetAllCards()
    {
        // Get total amount of cards per deck
        // Cards per deck should be a fixed value (i.e. 10)
        for (int i = 0; i < 10; i++)
        {
            // Photon scene instantiate
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

    public void Draw(int cardsToDraw)
    {
        for (int i = 0; i < cardsToDraw; i++)
        {
            hand.Add(deck[i]);
        }
    }
}
