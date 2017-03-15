using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public string playerName;
    // FIX: album and deck data are populated during sign-in
    //public List<Card> album;
    //public List<Card> deck;
    public Deck deck;
    public List<Card> hand;
    //public GameObject cardPrefab;
    //public GameObject deckParent;
    public GameObject handParent;

    /************
    /// <summary>
    /// Gets total # of cards player has and instantiates each one with a Card class
    /// </summary>
    public IEnumerator GetPlayerData(string url)
    {
        deckParent = new GameObject();
        deckParent.name = "Deck";
        deckParent.AddComponent<RectTransform>();
        deckParent.transform.SetParent(transform);
        deckParent.transform.localPosition = Vector2.zero;

        handParent = new GameObject();
        handParent.name = "Hand";
        handParent.transform.SetParent(transform);
        handParent.AddComponent<RectTransform>();
        handParent.transform.SetParent(transform);
        handParent.transform.localPosition = Vector2.zero;

        WWW www = new WWW(url);
        yield return www;

        string[] playerData = www.text.Split("|"[0]);
        name = playerData[1];

        // Split to get individual card data
        // id~name~sprite~attack~effect$
        // 1~Apple~~1,2,3,4~$
        string[] deckData = playerData[2].Split("$"[0]);
        string[] albumData = playerData[3].Split("$"[0]);

        StartCoroutine(PopulateDeck(deckData));
        // PopulateAlbum with albumData

        Shuffle();
        Draw(5);
    }

    IEnumerator LoadImage(string url, Sprite cardSprite)
    {
        WWW www = new WWW(url);
        yield return www;

        cardSprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);
    }

    IEnumerator PopulateDeck(string[] deckData)
    {
        for (int i = 0; i < deckData.Length; i++)
        {
            GameObject card = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            card.transform.SetParent(deckParent.transform);
            card.transform.localPosition = Vector2.zero;

            string[] cardData = deckData[i].Split("~"[0]);
            card.GetComponent<Card>().id = int.Parse(cardData[0]);
            card.name = cardData[1];

            if (!string.IsNullOrEmpty(cardData[2]))
            {
                yield return StartCoroutine(LoadImage(cardData[2], card.GetComponent<Card>().sprite));
            }

            string[] attackData = cardData[3].Split(","[0]);
            card.GetComponent<Card>().attack.Add("top", int.Parse(attackData[0]));
            card.GetComponent<Card>().attack.Add("right", int.Parse(attackData[1]));
            card.GetComponent<Card>().attack.Add("bottom", int.Parse(attackData[2]));
            card.GetComponent<Card>().attack.Add("left", int.Parse(attackData[3]));

            deck.Add(card.GetComponent<Card>());

            if (!string.IsNullOrEmpty(cardData[4]))
            {
                card.GetComponent<Card>().effect = cardData[4];
            }
        }
    }

    void PopulateAlbum(string cardData, GameObject card)
    {

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
    **************/

    public IEnumerator Initialize()
    {
        handParent = new GameObject();
        handParent.name = "Hand";
        handParent.transform.SetParent(transform);
        handParent.AddComponent<RectTransform>();
        handParent.transform.SetParent(transform);
        handParent.transform.localPosition = Vector2.zero;
        handParent.transform.localScale = new Vector2(1, 1);
        yield return null;
    }

    /// <summary>
    /// Draw x number of cards from deck.
    /// </summary>
    /// <param name="cardsToDraw"># of cards to draw from deck to be added to hand</param>
    public IEnumerator Draw(int cardsToDraw)
    {
        if (!IsHandFull())
        {
            // Get +/- value based on player position
            float sign = Mathf.Sign(transform.localPosition.x) * -1;
            float newX = sign * 150;
            float newY = 600f;
            float newRotateZ = sign * 22.5f;

            int endDeckIndex = deck.currentCardIndex + cardsToDraw;
            int handIndex = 0;

            for (int currentDeckIndex = deck.currentCardIndex; currentDeckIndex < endDeckIndex; currentDeckIndex++)
            {
                if (hand.Count < 5)
                {
                    hand.Add(deck.cardList[currentDeckIndex]);
                }
                else
                {
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (hand[i] == null)
                        {
                            hand[i] = deck.cardList[currentDeckIndex];
                            handIndex = i;
                            break;
                        }
                    }
                }
                hand[handIndex].transform.SetParent(handParent.transform);
                hand[handIndex].SetOwner(transform);
                hand[handIndex].transform.SetSiblingIndex(handIndex);

                Vector2 centeredPos = new Vector2((-(transform.localPosition.x) + transform.GetComponent<RectTransform>().anchoredPosition.x) * 2, (deck.transform.localPosition.y * 2) - (deck.GetComponent<RectTransform>().sizeDelta.y / 2));

                hand[handIndex].transform.localPosition = centeredPos;
                hand[handIndex].transform.localScale = new Vector2(1, 1);

                hand[handIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
                hand[handIndex].GetComponent<Image>().color = Color.white;

                hand[handIndex].ShowAttackUI();

                deck.cardList[currentDeckIndex] = null;

                LeanTween.moveLocal(hand[handIndex].gameObject, Vector2.zero, 0.15f)
                    .setEase(LeanTweenType.easeInQuad);

                deck.UpdateTotalCards();
                yield return new WaitForSeconds(0.15f);

                if (handIndex == 0)
                {
                    newX = sign * 150;
                    newY = 600f;
                    newRotateZ = sign * 22.5f;
                }
                else if (handIndex == 1)
                {
                    newX = sign * 250;
                    newY = 300f;
                    newRotateZ = sign * 11.25f;
                }
                else if (handIndex == 2)
                {
                    newX = sign * 275;
                    newY = 0;
                    newRotateZ = 0;
                }
                else if (handIndex == 3)
                {
                    newX = sign * 250;
                    newY = -300f;
                    newRotateZ = sign * -11.25f;
                }
                else if (handIndex == 4)
                {
                    newX = sign * 150;
                    newY = -600f;
                    newRotateZ = sign * -22.5f;
                }

                LeanTween.moveLocal(hand[handIndex].gameObject, new Vector2(newX, newY), 0.2f)
                    .setEase(LeanTweenType.easeInQuad);
                LeanTween.value(hand[handIndex].gameObject, Vector3.zero, new Vector3(0, 0, newRotateZ), 0.2f)
                    .setOnUpdateVector3((Vector3 val) =>
                    {
                        hand[handIndex].transform.localEulerAngles = val;
                    });

                yield return new WaitForSeconds(0.2f);
                handIndex++;
            }
        }
        else
        {
            Debug.Log(playerName + " hand is full.");
        }
    }

    bool IsHandFull()
    {
        if (hand.Count >= 5)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] == null)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
