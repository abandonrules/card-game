using UnityEngine;
using UnityEngine.EventSystems;
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
    //
    public int turnCount;
    // Reference to Card controls
    private Button rotateButton;
    private Button placeButton;

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
        SpawnCardControls();
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
            players[i].gameManager = this;
            yield return StartCoroutine(players[i].Initialize());
            yield return StartCoroutine(players[i].Draw(5));
            //StartCoroutine(player.GetPlayerData("http://byroncustodio.com/unity/card-game/card.php?request=PLAYER&id=" + player.id));
        }
    }

    public IEnumerator UpdateTurnCount()
    {
        turnCount++;
        yield return null;
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
            players[0].selectedCard.Rotate(true);
        }
    }

    public void MoveSelectedCardToBoard(Transform cell)
    {
        if (cell.GetComponent<Image>().color == Color.green)
        {
            // Add case: If cell already has card
            if (players[0].selectedCard != null)
            {
                Debug.Log("Placing card.");

                Card selectedCard = players[0].selectedCard;
                players[0].selectedCard = null;
                for(int i = 0; i < players[0].hand.Count; i++)
                {
                    if (players[0].hand[i] == selectedCard)
                    {
                        players[0].hand[i] = null;
                        break;
                    }
                }

                selectedCard.transform.SetParent(cell);
                selectedCard.Place(Vector2.zero);
            }
            else
            {
                Debug.Log("Select a card first.");
            }
        }
        else if (cell.GetComponent<Image>().color == Color.white)
        {
            Debug.Log("Invalid place. Choose a valid place.");
        }
    }
}
