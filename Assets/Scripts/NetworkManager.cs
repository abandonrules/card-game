using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : Photon.MonoBehaviour {

    const string GameVersion = "v0.01";

    private string clientPlayerName;

	public Button join;
    public Player player;
    public TextMeshProUGUI networkMessageUI;

    /// <summary>
    /// Handles any messages regarding Photon networking.
    /// </summary>
    /// <param name="newMessage">The newest message being passed.</param>
    private void SetNetworkMessage(string newMessage)
    {
        if (networkMessageUI.text == "")
        {
            networkMessageUI.text = newMessage;
            return;
        }

        if (LeanTween.isTweening(networkMessageUI.rectTransform))
        {
            LeanTween.cancel(networkMessageUI.rectTransform);
        }

        string currentMessage = networkMessageUI.text;

        // Remove current message rich text
        if (currentMessage.Contains("</color>"))
        {
            // Removes <color=#ffffff>
            int firstIndex = currentMessage.IndexOf("<");
            int lastIndex = currentMessage.IndexOf(">");
            currentMessage = currentMessage.Substring(0, firstIndex) + currentMessage.Substring(lastIndex + 1, currentMessage.Length - lastIndex - 1);

            // Removes </color>
            firstIndex = currentMessage.IndexOf("<");
            currentMessage = currentMessage.Substring(0, firstIndex);
        }
        networkMessageUI.text = currentMessage + "\n" + newMessage;

        while(networkMessageUI.preferredHeight > networkMessageUI.rectTransform.sizeDelta.y)
        {
            int firstMessageIndex = networkMessageUI.text.IndexOf("\n");
            networkMessageUI.text = networkMessageUI.text.Substring(firstMessageIndex + 1);
        }

        int lastMessageIndex = networkMessageUI.text.LastIndexOf("\n");
        string lastMessage = networkMessageUI.text.Substring(lastMessageIndex + 1);

        string red = Mathf.RoundToInt(networkMessageUI.color.r * 255).ToString("X");
        string green = Mathf.RoundToInt(networkMessageUI.color.g * 255).ToString("X");
        string blue = Mathf.RoundToInt(networkMessageUI.color.b * 255).ToString("X");

        string cutMessage = networkMessageUI.text.Substring(0, networkMessageUI.text.Length - lastMessage.Length);

        LeanTween.value(networkMessageUI.gameObject, 25f, 255f, 1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
                string alpha = Mathf.RoundToInt(val).ToString("X");

                string hex = "#" + red + green + blue + alpha;
                string modifiedLastMessage = "<color=" + hex.ToLower() + ">" + lastMessage + "</color>";
                networkMessageUI.text = cutMessage + modifiedLastMessage;
            })
            .setLoopPingPong(-1);

        return;
    }

    /// <summary>
    /// Raises the join event when attempting to connect with Photon.
    /// </summary>
    /// <param name="name">Name of the player joining.</param>
    public void OnClickJoin(InputField name)
    {
        if (join.GetComponentInChildren<TextMeshProUGUI>().text.Equals("JOIN"))
        {
            clientPlayerName = name.text;

            name.interactable = false;
            join.GetComponentInChildren<TextMeshProUGUI>().text = "CANCEL";

            SetNetworkMessage("Connecting to server...");
            ConnectToPhoton();
        }
        else
        {
            DisconnectFromServer();
        }
    }

    /// <summary>
    /// Connects to server.
    /// </summary>
    public static void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings(GameVersion);
    }

    /// <summary>
    /// Disconnects from server.
    /// </summary>
    public static void DisconnectFromServer()
    {
        GameObject nameField = GameObject.Find("Name Input");
        nameField.GetComponent<InputField>().interactable = true;

        GameObject joinButton = GameObject.Find("Join Button");
        joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "JOIN";

        GameObject networkText = GameObject.Find("Network Status");
        LeanTween.cancel(networkText);
        networkText.GetComponent<TextMeshProUGUI>().text = "";

        PhotonNetwork.Disconnect();
    }

    void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        SetNetworkMessage("Connection established.");
    }

    void OnJoinedRoom()
    {
        // FIX: Instantiate should be done at start of app
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        GameObject playerGO = PhotonNetwork.player.TagObject as GameObject;
        playerGO.name = clientPlayerName;
        player = playerGO.GetComponent<Player>();

        //Debug.Log("Joined room. Searching for an opponent.");

        Hashtable playerProps = new Hashtable();
        playerProps.Add("Name", clientPlayerName);
        playerProps.Add("Rank", 1);
        playerProps.Add("Level", Random.Range(1, 10));

        Color playerColor = GetComponent<MenuManager>().colorPanel.selectedColor;
        string serializedColor = playerColor.r + "," + playerColor.g + "," + playerColor.b;
        playerProps.Add("Color", serializedColor);
        PhotonNetwork.SetPlayerCustomProperties(playerProps);
        PhotonNetwork.playerName = clientPlayerName;
    }

    [PunRPC]
    public void StartGame()
    {
        SetNetworkMessage("Found opponent. Starting game in...");

        int i = 5;
        SetNetworkMessage(i.ToString());
        LeanTween.delayedCall(1f, () =>
        {
            i--;
            if (i <= 0)
            {
                PhotonNetwork.LoadLevel("Test_Gameplay");
            }
            else
            {
                SetNetworkMessage(i.ToString());
            }
        })
        .setRepeat(5);
    }

    [PunRPC]
    public void ResetSearch()
    {
        Debug.Log("Opponent disconnected.");
        SetNetworkMessage("<color=red>Error: Failed to start match.</color>\nSearching for an opponent...");
    }

    void OnPhotonJoinRoomFailed()
    {
        Debug.Log("Failed to join/create room.");
    }

    void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        SetNetworkMessage("Searching for an opponent...");
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
