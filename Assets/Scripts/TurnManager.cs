using UnityEngine;
using TMPro;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TurnManager : Photon.MonoBehaviour {

    public TextMeshProUGUI turnCountUI;
    public TextMeshProUGUI turnTimerUI;
    public TextMeshProUGUI turnPlayerUI;

    private float turnDuration = 45f;

    public int Turn
    {
        get
        {
            return PhotonNetwork.room.GetTurn();
        }
    }

    public string CurrentPlayer
    {
        get
        {
            return PhotonNetwork.room.GetCurrentPlayer();
        }
    }

    public float ElapsedTimeInTurn
    {
        get 
        {
            return ((float)PhotonNetwork.ServerTimestamp - PhotonNetwork.room.GetTurnStart()) / 1000f;
        }
    }

    public float RemainingSecondsInTurn
    {
        get
        {
            return Mathf.RoundToInt(Mathf.Max(0f, this.turnDuration - this.ElapsedTimeInTurn));
        }
    }

    public bool TimeEnd
    {
        get
        {
            return this.RemainingSecondsInTurn <= 0;
        }
    }

    private bool isTimeEnded = false;

    void Update()
    {
        if (Turn > 0 && this.TimeEnd && !isTimeEnded)
        {
            //Debug.Log("Timer ended.");
            isTimeEnded = true;
            turnTimerUI.text = "Time: 0s";
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.EndTurn();
            }
        }
        else if (Turn > 0 && !isTimeEnded)
        {
            //Debug.Log(this.RemainingSecondsInTurn);
            turnTimerUI.text = "Time: " + this.RemainingSecondsInTurn.ToString() + "s";
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (PhotonNetwork.player.NickName.Equals(this.CurrentPlayer))
            {
                photonView.RPC("MasterEndTurn", PhotonTargets.MasterClient, null);
            }
            else
            {
                Debug.Log("Can't end turn. Not your turn.");
            }
        }
    }

    public void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("TurnCount"))
        {
            isTimeEnded = false;
            turnCountUI.text = "Turn: " + Turn.ToString();
            Debug.Log("Turn: " + Turn);
        }

        if (propertiesThatChanged.ContainsKey("TurnPlayer"))
        {
            turnPlayerUI.text = "Current Turn: " + this.CurrentPlayer;
        }

        if (propertiesThatChanged.ContainsKey("EndMove"))
        {
            if((bool)propertiesThatChanged["EndMove"])
            {
                isTimeEnded = true;
                Debug.Log(this.CurrentPlayer + " ended their turn.");
                if (PhotonNetwork.isMasterClient)
                {
                    StartCoroutine(WaitToStartNextTurn());
                }
            }
        }
    }

    IEnumerator WaitToStartNextTurn()
    {
        // Set player of next turn
        string nextPlayer = "";
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            if (this.CurrentPlayer.Equals(PhotonNetwork.playerList[i].NickName))
            {
                continue;
            }
            else
            {
                nextPlayer = PhotonNetwork.playerList[i].NickName;
                break;
            }
        }

        yield return new WaitForSeconds(3);
        PhotonNetwork.room.SetCurrentPlayer(nextPlayer);
        PhotonNetwork.room.NextTurn();
    }

    [PunRPC]
    void MasterEndTurn()
    {
        PhotonNetwork.room.EndTurn();
    }
}

public static class TurnExtensions {

    /// <summary>
    /// The current turn number
    /// </summary>
    public static readonly string TurnCountKey = "TurnCount";

    /// <summary>
    /// The current player of the turn
    /// </summary>
    public static readonly string TurnPlayerKey = "TurnPlayer";

    /// <summary>
    /// Start (server) time of turn (used to calculate end)
    /// </summary>
    public static readonly string TurnStartKey = "TurnStart";

    /// <summary>
    /// Identifier to check if turn ended
    /// </summary>
    public static readonly string TurnEndMoveKey = "EndMove";

    public static void BeginInitialTurn(this Room room)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        string randomPlayer = "";
        if (!room.CustomProperties.ContainsKey(TurnPlayerKey))
        {
            randomPlayer = PhotonNetwork.playerList[Random.Range(0, PhotonNetwork.playerList.Length)].NickName;
        }
        else
        {
            Debug.LogError("Error in choosing random player.");
            Debug.Break();
        }
        PhotonNetwork.room.SetCurrentPlayer(randomPlayer);

        Hashtable roomProps = new Hashtable();
        roomProps.Add(TurnCountKey, 1);
        roomProps.Add(TurnStartKey, PhotonNetwork.ServerTimestamp);
        room.SetCustomProperties(roomProps);
    }

	public static void NextTurn(this Room room)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        int turn = 0;

        if (room.CustomProperties.ContainsKey(TurnCountKey))
        {
            turn = (int)room.CustomProperties[TurnCountKey];
        }
        turn++;

        // Set Turn
        roomProps.Add(TurnCountKey, turn);
        // Set start of Turn
        roomProps.Add(TurnStartKey, PhotonNetwork.ServerTimestamp);
        room.SetCustomProperties(roomProps);
    }

    public static int GetTurn(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnCountKey))
        {
            return 0;
        }

        return (int)room.CustomProperties[TurnCountKey];
    }

    public static void SetCurrentPlayer(this Room room, string nextPlayer)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        roomProps.Add(TurnPlayerKey, nextPlayer);
        room.SetCustomProperties(roomProps);
    }

    public static string GetCurrentPlayer(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPlayerKey))
        {
            return null;
        }

        return room.CustomProperties[TurnPlayerKey].ToString();
    }

    public static int GetTurnStart(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnStartKey))
        {
            return 0;
        }

        return (int)room.CustomProperties[TurnStartKey];
    }

    public static void EndTurn(this Room room)
    {
        if (room == null || room.CustomProperties == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        roomProps.Add(TurnEndMoveKey, true);
        room.SetCustomProperties(roomProps);
        //Debug.Log("Turn ended.");
    }
}
