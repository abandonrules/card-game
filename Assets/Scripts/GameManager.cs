using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    // Reference to players in the game
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

    // Reference to Card controls
    private Button rotateButton;
    private Button placeButton;

    void Start()
    {
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);

        if (PhotonNetwork.isMasterClient)
        {
            LeanTween.delayedCall(5f, () =>
            {
                photonView.RPC("Initialize", PhotonTargets.AllViaServer, null);
            });
        }
    }

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

    void BeginGame()
    {
        Debug.Log("Starting game...");
        PhotonNetwork.room.BeginInitialTurn();
    }

    void SpawnCardControls()
    {
        GameObject rotateGO = Instantiate(Resources.Load<GameObject>("Rotate Button"), Vector2.zero, Quaternion.identity, canvas.transform) as GameObject;
        rotateButton = rotateGO.GetComponent<Button>();
        rotateButton.name = "Rotate Button";
        rotateButton.onClick.AddListener(() => { OnClickCardControl(rotateButton); });
        Text rotateText = rotateButton.GetComponentInChildren<Text>();
        rotateText.name = "Text";
        rotateText.text = "Rotate";
        rotateButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-475, 150f);
        rotateButton.gameObject.SetActive(false);

        GameObject placeGO = Instantiate(Resources.Load<GameObject>("Place Button"), Vector2.zero, Quaternion.identity, canvas.transform) as GameObject;
        placeButton = placeGO.GetComponent<Button>();
        placeButton.name = "Place Button";
        placeButton.onClick.AddListener(() => { OnClickCardControl(placeButton); });
        Text placeText = placeButton.GetComponentInChildren<Text>();
        placeText.name = "Text";
        placeText.text = "Place";
        placeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-475, -150f);
        placeButton.gameObject.SetActive(false);
    }

    public void ShowCardControls()
    {
        rotateButton.gameObject.SetActive(true);
        placeButton.gameObject.SetActive(true);
    }

    public void HideCardControls()
    {
        rotateButton.gameObject.SetActive(false);
        placeButton.gameObject.SetActive(false);
    }

    public void OnClickCardControl(Button button)
    {
        if (button.name.Equals("Rotate Button"))
        {
            playerList[0].selectedCard.Rotate(true);
        }
    }
}
