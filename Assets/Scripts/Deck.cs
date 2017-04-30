using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Deck : MonoBehaviour {

    public static readonly int MAX_TILES = 80;

    public int Total
    {
        get
        {
            return PhotonNetwork.room.GetDeckTotal();
        }
    }

    public string DeckAttack
    {
        get
        {
            return PhotonNetwork.room.GetDeckAttack();
        }
    }

    public string PlayerInteracting
    {
        get
        {
            return PhotonNetwork.room.GetDeckInteract();
        }
    }

    public bool IsAllPlayersReady
    {
        get
        {
            return PhotonNetwork.room.GetInitialPlayerStatus();
        }
    }

    public GameManager gameManager;
    public GameObject cardPrefab;
    public List<Card> cardList;

    public static void AssignCardValues()
    {
        List<string> tempAttack = new List<string>();
        int attackTop = 1;
        int attackRight = 1;
        int attackBottom = 1;
        int attackLeft = 1;

        for (int i = 1; i < 500; i++)
        {
            tempAttack.Add(attackTop.ToString() + "," + attackRight.ToString() + "," + attackBottom.ToString() + "," + attackLeft.ToString());

            if (attackRight == 5 && attackBottom == 5 && attackLeft == 5)
            {
                attackTop++;
                attackRight = 1;
                attackBottom = 1;
                attackLeft = 1;
                continue;
            }
            else if (attackBottom == 5 && attackLeft == 5)
            {
                attackRight++;
                attackBottom = 1;
                attackLeft = 1;
                continue;
            }
            else if (attackLeft == 5)
            {
                attackBottom++;
                attackLeft = 1;
                continue;
            }
            else
            {
                attackLeft++;
            }
        }

        ShuffleTemp(tempAttack);
    }

    static void ShuffleTemp(List<string> tempAttack)
    {
        for (int i = 0; i < tempAttack.Count; i++)
        {
            int randomIndex = Random.Range(0, tempAttack.Count);
            string newIndex = tempAttack[randomIndex];
            tempAttack[randomIndex] = tempAttack[i];
            tempAttack[i] = newIndex;
        }

        string serializedAttack = "";

        // Only 80/500 randomized attacks are sent to server
        for (int i = 0; i < MAX_TILES; i++)
        {
            serializedAttack += tempAttack[i] + "|";
        }
        PhotonNetwork.room.SetDeckAttack(serializedAttack);
    }

    public void Create()
    {
        string[] attackList = this.DeckAttack.Split("|"[0]);
        for (int i = 0; i < MAX_TILES; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            Card card = cardGO.GetComponent<Card>();
            card.name = i.ToString();

            string[] randAttack = attackList[i].Split(","[0]);
            card.attack["Top"] = int.Parse(randAttack[0]);
            card.attack["Right"] = int.Parse(randAttack[1]);
            card.attack["Bottom"] = int.Parse(randAttack[2]);
            card.attack["Left"] = int.Parse(randAttack[3]);

            cardList.Add(card);

            foreach(Transform child in card.transform)
            {
                child.GetComponent<TextMeshProUGUI>().color = Color.clear;
                child.GetComponent<TextMeshProUGUI>().text = card.attack[child.name].ToString();
                child.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                card.attackUI.Add(child.GetComponent<TextMeshProUGUI>());
            }

            // FIX: Change based on resolution
            cardGO.transform.localPosition = new Vector2(1000, -500);

            card.board = FindObjectOfType<Board>();
            card.SetOwner(transform);
        }

        Debug.Log("Spawned playing cards...");

        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.SetDeckTotal(cardList.Count);
            gameManager.photonView.RPC("DealInitialCards", PhotonTargets.MasterClient, null);
        }
    }

    public void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("DeckTotal"))
        {
            GetComponent<TextMeshProUGUI>().text = "Deck: " + this.Total;
            //Debug.Log("Deck Count: " + this.Total);
        }

        if (propertiesThatChanged.ContainsKey("DeckAttack"))
        {
            Debug.Log("Creating deck...");
            Create();
        }

        if (propertiesThatChanged.ContainsKey("DeckInteract"))
        {
            // Player is done interacting
            if (propertiesThatChanged["DeckInteract"].Equals(""))
            {
                Debug.Log("Deck is free.");
            }
            else
            {
                Debug.Log(propertiesThatChanged["DeckInteract"].ToString() + " using deck.");
            }
        }
    }
}

public static class DeckExtensions {

    public static readonly string DeckTotalKey = "DeckTotal";

    public static readonly string DeckAttackKey = "DeckAttack";

    public static readonly string DeckInteractKey = "DeckInteract";

    public static readonly string PlayerReadyKey = "PlayerReady";

    public static void SetDeckTotal(this Room room, int amount)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        roomProps.Add(DeckTotalKey, amount);
        room.SetCustomProperties(roomProps);
    }

    public static int GetDeckTotal(this RoomInfo room)
    {
        object total;

        if (room.CustomProperties.TryGetValue(DeckTotalKey, out total))
        {
            return (int)total;
        }

        return 0;
    }

    public static void SetDeckAttack(this Room room, string deckAttack)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        roomProps.Add(DeckAttackKey, deckAttack);
        room.SetCustomProperties(roomProps);
    }

    public static string GetDeckAttack(this RoomInfo room)
    {
        object attack;

        if (room.CustomProperties.TryGetValue(DeckAttackKey, out attack))
        {
            return attack.ToString();
        }

        return null;
    }

    public static void SetDeckInteract(this Room room, PhotonPlayer player = null)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        if (player != null)
        {
            roomProps.Add(DeckInteractKey, player.NickName);
        }
        else
        {
            roomProps.Add(DeckInteractKey, "");
        }
        room.SetCustomProperties(roomProps);
    }

    public static string GetDeckInteract(this RoomInfo room)
    {
        object player;

        if (room.CustomProperties.TryGetValue(DeckInteractKey, out player))
        {
            return player.ToString();
        }

        return null;
    }

    public static void SetInitialPlayerStatus(this Room room, PhotonPlayer player = null)
    {
        if (room == null)
        {
            Debug.LogError("Check if client is in room or is connected.");
            Debug.Break();
        }

        Hashtable roomProps = new Hashtable();
        if (player == null)
        {
            Debug.LogError("SetInitialPlayerStatus() missing player parameter of type PhotonPlayer.");
            Debug.Break();
        }
        else
        {
            object otherPlayer;

            if (room.CustomProperties.TryGetValue(PlayerReadyKey, out otherPlayer))
            {
                string bothPlayers = otherPlayer.ToString() + "," + player.NickName;
                roomProps.Add(PlayerReadyKey, bothPlayers);
            }
            else
            {
                roomProps.Add(PlayerReadyKey, player.NickName);
            }

        }
        room.SetCustomProperties(roomProps);
    }

    public static bool GetInitialPlayerStatus(this RoomInfo room)
    {
        object players;

        if (room.CustomProperties.TryGetValue(PlayerReadyKey, out players))
        {
            if (players.ToString().Contains(","))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    } 
}
