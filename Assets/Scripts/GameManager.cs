using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public Board board;
    public List<GameObject> players;

    void Start()
    {
        board.Create();
        CenterCamera();
        GetPlayers();
    }

    // Camera is centered depending if board size is even/odd
    /*void CenterCamera()
    {
        if (board.width % 2 == 0)
        {
            Camera.main.transform.localPosition = new Vector3((board.width - 1) / 2f, (board.height - 1) / 2f, Camera.main.transform.localPosition.z);
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(Mathf.FloorToInt(board.width / 2), Mathf.FloorToInt(board.height / 2), Camera.main.transform.localPosition.z);
        }
    }
    */

    void CenterCamera()
    {
        // -50, -75, -100
        board.GetComponent<RectTransform>().localPosition = new Vector2();
    }

    void GetPlayers()
    {
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        // Test code. Set name when players join room
        players[0].name = "Player 1";
        players[1].name = "Player 2";

        foreach(GameObject player in players)
        {
            player.GetComponent<Player>().GetAllCards();
        }
    }
}
