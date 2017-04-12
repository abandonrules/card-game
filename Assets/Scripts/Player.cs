using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : Photon.MonoBehaviour {

    public Deck deck
    {
        get
        {
            return FindObjectOfType<Deck>();
        }
    }
    public List<Card> hand;
    public Card selectedCard;

    public bool isHandFull
    {
        get
        {
            return CheckHand();
        }
    }

    public GameObject handParent;
    public GameManager gameManager
    {
        get
        {
            return FindObjectOfType<GameManager>();
        }
    }

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.sender.TagObject = this.gameObject;
    }

    public void Initialize()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        transform.localScale = Vector2.one;

        handParent = new GameObject();
        handParent.name = "Hand";
        handParent.transform.SetParent(transform);
        handParent.AddComponent<RectTransform>();
        handParent.transform.localPosition = Vector2.zero;
        handParent.transform.localScale = Vector2.one;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Test_Gameplay")
        {
            if (gameManager.turnManager.TimeEnd && photonView.isMine && gameManager.turnManager.CurrentPlayer == PhotonNetwork.player.NickName && selectedCard != null)
            {
                UndoSelectMove();
            }
        }
    }

    /// <summary>
    /// Draw x number of cards from deck.
    /// </summary>
    /// <param name="cardsToDraw"># of cards to draw from deck to be added to hand</param>
    [PunRPC]
    public IEnumerator Draw(int cardsToDraw, PhotonMessageInfo info)
    {
        // local client
        if (photonView.isMine)
        {
            PhotonNetwork.room.SetDeckInteract(PhotonNetwork.player);

            float sign = Mathf.Sign(transform.localPosition.x) * -1;
            float newX = sign * 100;
            float newY = 300f;

            // have card method to move to player hand???
            for (int i = 0; i < cardsToDraw; i++)
            {
                // Initial draw
                if (hand.Count < 5)
                {
                    hand.Add(deck.cardList[0]);
                }
                else
                {
                    // Recurring draws
                    for (int j = 0; j < hand.Count; j++)
                    {
                        if (hand[j] == null)
                        {
                            i = j;
                            hand[j] = deck.cardList[0];
                            break;
                        }
                    }
                }

                hand[i].transform.SetParent(handParent.transform);
                hand[i].SetOwner(transform);
                hand[i].transform.SetSiblingIndex(i);

                Vector2 centeredPos = new Vector2((-(transform.localPosition.x) + transform.GetComponent<RectTransform>().anchoredPosition.x) * 2, 
                                                    (deck.transform.localPosition.y * 2) - (deck.GetComponent<RectTransform>().sizeDelta.y / 2));

                hand[i].transform.localPosition = centeredPos;
                hand[i].transform.localScale = new Vector2(1, 1);
                hand[i].GetComponent<Image>().color = Color.white;
                hand[i].ShowAttackUI();

                deck.cardList.RemoveAt(0);
                PhotonNetwork.room.SetDeckTotal(deck.cardList.Count);

                if (i == 0)
                {
                    newY = 300f;
                }
                else if (i == 1)
                {
                    newY = 150f;
                }
                else if (i == 2)
                {
                    newY = 0;
                }
                else if (i == 3)
                {
                    newY = -150f;
                }
                else if (i == 4)
                {
                    newY = -300f;
                }

                LeanTween.value(hand[i].gameObject, hand[i].transform.localPosition, new Vector3(newX, newY, 0), 0.15f)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnUpdateVector3((Vector3 val) =>
                    {
                        hand[i].GetComponent<RectTransform>().anchoredPosition = val;
                    });
                yield return new WaitForSeconds(0.15f);
            }

            PhotonNetwork.room.SetDeckInteract(null);
            PhotonNetwork.room.SetInitialPlayerStatus(PhotonNetwork.player);
        }
        else // remote
        {
            Debug.Log(PhotonNetwork.otherPlayers[0].NickName + " drawing " + cardsToDraw + " cards.");
            for (int i = 0; i < cardsToDraw; i++)
            {
                if (hand.Count < 5)
                {
                    hand.Add(deck.cardList[0]);
                }
                else
                {
                    for (int j = 0; j < hand.Count; j++)
                    {
                        if (hand[j] == null)
                        {
                            i = j;
                            hand[j] = deck.cardList[0];
                            break;
                        }
                    }
                }

                hand[i].transform.SetParent(handParent.transform);
                hand[i].SetOwner(transform);
                hand[i].transform.SetSiblingIndex(i);

                deck.cardList.RemoveAt(0);
            }
        }
    }

    bool CheckHand()
    {
        if (hand != null)
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
        else
        {
            return false;
        }
    }

    public void SelectCard(Card card)
    {
        if (gameManager.turnManager.CurrentPlayer == PhotonNetwork.player.NickName)
        {
            if (selectedCard == null)
            {
                card.board.AddCoverListener(this);
                selectedCard = card;
                card.Select();
            }
            else if (card != selectedCard)
            {
                UndoSelectMove();
                selectedCard = card;
                selectedCard.Select();
            }
            else if (card == selectedCard)
            {
                UndoSelectMove();
            }
        }
    }

    public void PlaceCard(Cell cell)
    {
        if (gameManager.turnManager.CurrentPlayer != PhotonNetwork.player.NickName)
        {
            return;
        }

        if (selectedCard != null)
        {
            if (cell.IsValid())
            {
                for(int i = 0; i < hand.Count; i++)
                {
                    if (hand[i] == selectedCard)
                    {
                        hand[i] = null;
                        break;
                    }
                }

                selectedCard.transform.SetParent(cell.transform);
                selectedCard.Place(Vector2.zero);
                selectedCard = null;
            }
            else
            {
                Debug.Log("Invalid move.");
            }
        }
        else
        {
            Debug.Log("Select a card first.");
        }
    }

    public void UndoSelectMove()
    {
        selectedCard.UndoPlayerSelect();
        selectedCard = null;
    }
}
