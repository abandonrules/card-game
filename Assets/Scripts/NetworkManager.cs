using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : Photon.MonoBehaviour {

    const string GameVersion = "v0.01";

    private string clientPlayerName;

	public Button join;
    public Player player;
    public Text networkMessageUI;

    void Awake()
    {
        // Only connect when needed
        //PhotonNetwork.ConnectUsingSettings(GameVersion);
    }

    public void OnClickJoin(InputField name)
    {
        if (join.GetComponentInChildren<Text>().text.Equals("JOIN"))
        {
            clientPlayerName = name.text;

            name.interactable = false;
            join.GetComponentInChildren<Text>().text = "CANCEL";
            networkMessageUI.text = "Connecting to server...";
            ConnectToServer();
        }
        else
        {
            //name.interactable = true;
            //join.GetComponentInChildren<Text>().text = "JOIN";
            //networkMessageUI.text = "";

            DisconnectFromServer();
        }
    }

    public static void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings(GameVersion);
    }

    public static void DisconnectFromServer()
    {
        GameObject nameField = GameObject.Find("Name Input");
        nameField.GetComponent<InputField>().interactable = true;

        GameObject joinButton = GameObject.Find("Join Button");
        joinButton.GetComponentInChildren<Text>().text = "JOIN";

        GameObject networkText = GameObject.Find("Network Status");
        networkText.GetComponent<Text>().text = "";

        PhotonNetwork.Disconnect();
    }

    void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        networkMessageUI.text = "Connection established.";
    }

    void OnJoinedRoom()
    {
        // FIX: Instantiate should be done at start of app
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        GameObject playerGO = PhotonNetwork.player.TagObject as GameObject;
        playerGO.name = clientPlayerName;
        player = playerGO.GetComponent<Player>();

        Debug.Log("Joined room. Searching for an opponent.");

        Hashtable playerProps = new Hashtable();
        playerProps.Add("Name", clientPlayerName);
        playerProps.Add("Rank", 1);
        playerProps.Add("Level", Random.Range(1, 10));
        PhotonNetwork.SetPlayerCustomProperties(playerProps);
        PhotonNetwork.playerName = clientPlayerName;
    }

    [PunRPC]
    public void StartGame()
    {
        networkMessageUI.text = "Found opponent. Starting game in...";

        int i = 3;
        Debug.Log(i);
        LeanTween.delayedCall(1f, () =>
        {
            i--;
            if (i <= 0)
            {
                PhotonNetwork.LoadLevel("Test_Gameplay");
            }
            Debug.Log(i);
        })
        .setRepeat(3);
    }

    [PunRPC]
    public void ResetSearch()
    {
        Debug.Log("Opponent disconnected.");
        networkMessageUI.text = "Searching for an opponent...";
    }

    void OnPhotonJoinRoomFailed()
    {
        Debug.Log("Failed to join/create room.");
    }

    void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        networkMessageUI.text = "Searching for an opponent...";

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom("", roomOptions, TypedLobby.Default);
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (PhotonNetwork.room.PlayerCount == 2)
            {
                PhotonNetwork.room.IsOpen = false;
                photonView.RPC("StartGame", PhotonTargets.AllViaServer, null);
            }
        }
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer leftPlayer)
    {
        if (PhotonNetwork.connected && PhotonNetwork.isMasterClient)
        {
            if (PhotonNetwork.room.PlayerCount < 2)
            {
                PhotonNetwork.room.IsOpen = true;
                photonView.RPC("ResetSearch", PhotonTargets.AllViaServer, null);
            }
        }
    }

    void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        // Make popup notif that opponent left.
        Debug.Log("Host left. Leaving room.");
        DisconnectFromServer();
    }
}
