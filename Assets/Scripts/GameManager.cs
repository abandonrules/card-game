using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Photon.MonoBehaviour {

    // Reference to TurnManager class
    public TurnManager turnManager
    {
        get
        {
            return GetComponent<TurnManager>();
        }
    }

    // Reference to Board class
    public Board board
    {
        get
        {
            return FindObjectOfType<Board>();
        }   
    }

    // Reference to Deck class
    public Deck deck
    {
        get
        {
            return FindObjectOfType<Deck>();
        }
    }

    // Reference to players and player objects in game
    public GameObject playerPrefab;
    public RectTransform playerParent;
    public List<Player> playerList;

    // Reference to Canvas
    public Canvas canvas
    {
        get
        {
            return FindObjectOfType<Canvas>();
        }
    }

    private Button rotateButton;

    /// <summary>
    /// Go back to Menu scene when not connected to Photon.
    /// </summary>
    void Awake()
    {
        if (!PhotonNetwork.connected)
        {
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Start of game that spawns both players and calls Initialize on both clients.
    /// </summary>
    void Start()
    {
        // wait for all players to join room
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);

        if (PhotonNetwork.isMasterClient)
        {
            LeanTween.delayedCall(5f, () =>
            {
                photonView.RPC("Initialize", PhotonTargets.AllViaServer, null);
            });
        }
    }

    /// <summary>
    /// Initialization of game; Creating the board, organizing players, and creating the deck.
    /// </summary>
    [PunRPC]
    IEnumerator Initialize()
    {
        Debug.Log("Setting up game...");
        yield return board.Create();
        SpawnCardControls();

        GameObject playerGO = PhotonNetwork.player.TagObject as GameObject;
        playerGO.name = PhotonNetwork.player.NickName;
        playerList.Add(playerGO.GetComponent<Player>());

        GameObject otherPlayerGO = GameObject.Find("Player(Clone)");
        if (otherPlayerGO == null)
        {
            Debug.LogError("Could not find opponent object.");
            Debug.Break();
        }
        otherPlayerGO.name = PhotonNetwork.otherPlayers[0].CustomProperties["Name"].ToString();
        playerList.Add(otherPlayerGO.GetComponent<Player>());

        foreach(Player player in playerList)
        {
            player.transform.SetParent(playerParent);
            player.Initialize();
        }

        if (PhotonNetwork.isMasterClient)
        {
            Deck.AssignCardValues();
        }
    }

    /// <summary>
    /// Deal cards to players (host being first), after setting card values from Deck.
    /// </summary>
    [PunRPC]
    public IEnumerator DealInitialCards()
    {
        if (!playerList[0].isHandFull)
        {
            playerList[0].photonView.RPC("Draw", PhotonTargets.AllViaServer, 5);
        }
        yield return new WaitUntil(() => deck.PlayerInteracting == "");
        if (!playerList[1].isHandFull)
        {
            playerList[1].photonView.RPC("Draw", PhotonTargets.AllViaServer, 5);  
        }
        yield return new WaitUntil(() => deck.IsAllPlayersReady == true);
        //  FIX: Add additional wait time to improve game start
        BeginGame();
    }

    /// <summary>
    /// Begin start of game. Start of Turn 1.
    /// </summary>
    void BeginGame()
    {
        Debug.Log("Starting game...");
        PhotonNetwork.room.BeginInitialTurn();
    }

    /// <summary>
    /// Spawn control properties of a selected card.
    /// </summary>
    void SpawnCardControls()
    {
        GameObject rotateGO = Instantiate(Resources.Load<GameObject>("Rotate Button"), Vector2.zero, Quaternion.identity, canvas.transform) as GameObject;
        rotateButton = rotateGO.GetComponent<Button>();
        rotateButton.name = "Rotate Button";
        rotateButton.onClick.AddListener(() => { OnClickCardControl(rotateButton); });
        TextMeshProUGUI rotateText = rotateButton.GetComponentInChildren<TextMeshProUGUI>();
        rotateText.name = "Text";
        rotateText.text = "ROTATE";
        rotateText.alignment = TextAlignmentOptions.Center;
        rotateButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-475, 150f);
        rotateButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show controls.
    /// </summary>
    public void ShowCardControls()
    {
        rotateButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide controls.
    /// </summary>
    public void HideCardControls()
    {
        rotateButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Event called when a card property button is clicked/tapped.
    /// </summary>
    /// <param name="button">Button that was pressed.</param>
    public void OnClickCardControl(Button button)
    {
        if (button.name.Equals("Rotate Button"))
        {
            if (!LeanTween.isTweening(playerList[0].selectedCard.gameObject))
            {
                playerList[0].selectedCard.Rotate(true);
            }
        }
    }

    /// <summary>
    /// Moves the current player's selected card to the board client-side.
    /// </summary>
    /// <param name="cardName">The selected card.</param>
    /// <param name="cardAttack">The selected card's attack sent as a string.</param>
    /// <param name="cellName">The target cell's name (position on the board).</param>
    /// <param name="info">The player who called this method.</param>
    [PunRPC]
    public IEnumerator MoveCardToBoard(string cardName, string cardAttack, string cellName, PhotonMessageInfo info)
    {
        Card selectedCard = null;
        Cell targetCell = null;

        foreach(Card card in FindObjectsOfType<Card>())
        {
            if (card.name == cardName)
            {
                selectedCard = card;
                break;
            }
        }

        foreach(Cell cell in FindObjectsOfType<Cell>())
        {
            if (cell.name == cellName)
            {
                targetCell = cell;
                break;
            }
        }

        if (selectedCard == null || targetCell == null)
        {
            Debug.LogError("Object Card or object Cell is missing a reference.");

            yield break;
        }

        selectedCard.transform.SetParent(targetCell.transform);
        selectedCard.transform.localScale = Vector2.one;
        selectedCard.ShowAttackUI();

        if (info.sender != PhotonNetwork.player)
        {
            for (int i = 0; i < playerList[1].hand.Count; i++)
            {
                if (playerList[1].hand[i].name == selectedCard.name)
                {
                    playerList[1].hand[i] = null;
                }
            }
            selectedCard.GetComponent<Image>().color = playerList[1].playerColor;
        }

        yield return StartCoroutine(selectedCard.Place(Vector2.zero, new Vector2(100, 100), cardAttack.Split(","[0])));

        if (info.sender == PhotonNetwork.player)
        {
            if (!playerList[0].isHandFull)
            {
                playerList[0].photonView.RPC("Draw", PhotonTargets.AllViaServer, 1);
            }
            board.ResetValidCells();
            // End turn after flipping tiles
            turnManager.photonView.RPC("MasterEndTurn", PhotonTargets.MasterClient, null);
        }

        Debug.Log(info.sender.NickName + " placed " + cardName + " on " + cellName);

        if (turnManager.Turn > 4)
        {
            List<Card> cardsToFlip = new List<Card>();
            cardsToFlip.AddRange(board.GetMatches(selectedCard, targetCell.name));

            foreach (Card card in cardsToFlip)
            {
                if (card.owner.name == PhotonNetwork.playerName)
                {
                    card.SetOwner(playerList[1].transform);
                    yield return StartCoroutine(card.Flip(playerList[1].playerColor));
                }
                else
                {
                    card.SetOwner(playerList[0].transform);
                    yield return StartCoroutine(card.Flip(playerList[0].playerColor));
                }
            }

            Debug.Log(info.sender.NickName + " stole " + cardsToFlip.Count + " cards.");
        }

    }

    /// <summary>
    /// Raises the photon player disconnected event when someone disconnects from the room.
    /// </summary>
    /// <param name="disconnectedPlayer">Disconnected player.</param>
    void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
    {
        // Add more when player disconnected
        GameObject eventPanelCover = GameObject.Find("Event Panel Cover");
        eventPanelCover.GetComponent<Image>().enabled = true;
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Raises the disconnected from photon event and sends everyone back to the Menu scene.
    /// </summary>
    void OnDisconnectedFromPhoton()
    {
        SceneManager.LoadScene(0);
    }
}
