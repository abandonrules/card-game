using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    // Reference to Board class
    public Board board;
    // Reference to Deck class
    public Deck deck;
    // Reference to both players in the game
    public List<Player> players;
    // Reference to Canvas
    public Canvas canvas;

    void Start()
    {
        StartCoroutine(Initialize());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(players[1].Draw(1));
        }
    }

    IEnumerator Initialize()
    {
        StartCoroutine(board.Create());
        yield return StartCoroutine(deck.Create());
        StartCoroutine(GetPlayers());
    }

    /// <summary>
    /// Store reference of both players.
    /// Start initialization of each player.
    /// </summary>
    IEnumerator GetPlayers()
    {
        players.AddRange(FindObjectsOfType<Player>());

        // FIX: Player data should be loaded during sign-in
        for (int i = players.Count - 1; i >= 0; i--)
        {
            players[i].deck = deck;
            yield return StartCoroutine(players[i].Initialize());
            yield return StartCoroutine(players[i].Draw(5));
            //StartCoroutine(player.GetPlayerData("http://byroncustodio.com/unity/card-game/card.php?request=PLAYER&id=" + player.id));
        }
    }
}
