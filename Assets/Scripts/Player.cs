using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Player : Photon.MonoBehaviour {

    public Color playerColor
    {
        get
        {
            return GetPlayerColor();
        }
    }

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

    private bool canPlaceRandomCard;

    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.sender.TagObject = this.gameObject;
    }

    public Color GetPlayerColor()
    {
        string[] colorVals = photonView.owner.CustomProperties["Color"].ToString().Split(","[0]);
        Color playerColor = new Color(float.Parse(colorVals[0]), float.Parse(colorVals[1]), float.Parse(colorVals[2]));
        return playerColor;
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
            if (!canPlaceRandomCard && gameManager.turnManager.TimeEnd && photonView.isMine && gameManager.turnManager.CurrentPlayer == PhotonNetwork.player.NickName)
            {
                canPlaceRandomCard = true;

                List<Cell> initialCells = new List<Cell>()
                {
                    gameManager.board.cells[3,3],
                    gameManager.board.cells[3,4],
                    gameManager.board.cells[4,3],
                    gameManager.board.cells[4,4]
                };

                if (selectedCard != null)
                {
                    // Place selected card
                    for (int i = 0; i < initialCells.Count; i++)
                    {
                        if (!initialCells[i].GetComponentInChildren<Card>())
                        {
                            PlaceCard(initialCells[i]);
                            break;
                        }
                    }
                }
                else
                {
                    // Place random card
                    for (int i = 0; i < initialCells.Count; i++)
                    {
                        if (!initialCells[i].GetComponentInChildren<Card>())
                        {
                            PlaceRandom(initialCells[i]);
                            break;
                        }
                    }
                }
            }
            if (canPlaceRandomCard && !gameManager.turnManager.TimeEnd && photonView.isMine && gameManager.turnManager.CurrentPlayer == PhotonNetwork.player.NickName)
            {
                canPlaceRandomCard = false;
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
                hand[i].transform.localScale = Vector2.one;
                hand[i].GetComponent<Image>().color = playerColor;
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
                //card.board.AddCoverListener(this);
                selectedCard = card;
                card.Select();
                return;
            }
            else if (card != selectedCard)
            {
                UndoSelectMove();
                selectedCard = card;
                selectedCard.Select();
                return;
            }
            else if (card == selectedCard)
            {
                UndoSelectMove();
                return;
            }
        }
    }

    // FIX: Add additional case where player can only place card adjacent to opponent card
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

                string selectedCardAttack = selectedCard.attack["Top"] + "," + selectedCard.attack["Right"] + "," + selectedCard.attack["Bottom"] + "," + selectedCard.attack["Left"];
                gameManager.photonView.RPC("MoveCardToBoard", PhotonTargets.AllViaServer, selectedCard.name, selectedCardAttack, cell.name);
                selectedCard = null;
                gameManager.HideCardControls();
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

    private void PlaceRandom(Cell cell)
    {
        if (gameManager.turnManager.CurrentPlayer != PhotonNetwork.player.NickName)
        {
            return;
        }

        int randomCard = Random.Range(0, hand.Count);
        selectedCard = hand[randomCard];
        hand[randomCard] = null;

        string[] selectedCardAttackList = new string[4];

        foreach(TextMeshProUGUI attack in selectedCard.attackUI)
        {
            if (attack.name == "Top")
            {
                selectedCardAttackList[0] = attack.text;
            }
            else if (attack.name == "Right")
            {
                selectedCardAttackList[1] = attack.text;
            }
            else if (attack.name == "Bottom")
            {
                selectedCardAttackList[2] = attack.text;
            }
            else if (attack.name == "Left")
            {
                selectedCardAttackList[3] = attack.text;
            }
        }

        string selectedCardAttack = selectedCardAttackList[0] + "," + selectedCardAttackList[1] + "," + selectedCardAttackList[2] + "," + selectedCardAttackList[3];
        gameManager.photonView.RPC("MoveCardToBoard", PhotonTargets.AllViaServer, selectedCard.name, selectedCardAttack, cell.name);
        selectedCard = null;
    }

    public void UndoSelectMove()
    {
        selectedCard.UndoPlayerSelect();
        selectedCard = null;
    }
}
