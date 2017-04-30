using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    // Prefab object to instantiate
    public GameObject cellPrefab;
    // Reference to all cell objects on the board
    public Cell[,] cells = new Cell[8, 8];  // FIX: Should depend on width/height vars
    // Reference to all Cards on the board
    public List<Card> cardList;
    //
    public GameManager gameManager;
    // Enumerator to determine size of board
    public enum Matrix
    {
        ThreeByThree,
        FourByFour,
        FiveByFive,
        SixBySix,
        EightByEight
    };
    public Matrix matrix;
    // Distance between each cell
    public float distance;
    // References to width and height of board
    public static int width;
    public static int height;

    private Image cover
    {
        get
        {
            return GameObject.Find("Board Cover").GetComponent<Image>();
        }
    }
    //private EventTrigger trigger;
    //private EventTrigger.Entry entry;

    // On awake, set the size of the board
    void Awake()
    {
        switch(matrix.ToString())
        {
            case "ThreeByThree":
                width = 3;
                height = 3;
                break;
            case "FourByFour":
                width = 4;
                height = 4;
                break;
            case "FiveByFive":
                width = 5;
                height = 5;
                break;
            case "SixBySix":
                width = 6;
                height = 6;
                break;
            case "EightByEight":
                width = 8;
                height = 8;
                break;
            default:
                width = 8;
                height = 8;
                break;
        };

        HideCover();
    }

    /// <summary>
    /// Create the board, instantiating cells for each position of the board
    /// </summary>
	public IEnumerator Create()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject cell = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
                cells[i,j] = cell.GetComponent<Cell>();
                cell.name = i + "," + j;

                // Offset cell so it's centered with Camera
                // Divide by 2 to offset board by only half the size of the board
                float offsetX = ((width - 1) * distance) / 2;
                float offsetY = ((height - 1) * distance) / 2;
                cell.GetComponent<RectTransform>().localPosition = new Vector2(i * distance - offsetX, j * distance - offsetY);
            }
        }

        yield return null;
    }

    /// <summary>
    /// Displays which cells on the board are valid for the player
    /// </summary>
    public void ShowValidCells()
    {
        ResetValidCells();

        List<Cell> validCells = new List<Cell>();
        validCells = GetValidCells(PhotonNetwork.player);

        foreach(Cell cell in validCells)
        {
            if (cell.GetComponent<Image>().color != Color.green)
            {
                cell.GetComponent<Image>().color = Color.green;
            }
        }
    }

    private List<Cell> GetValidCells(PhotonPlayer clientPlayer)
    {
        List<Cell> validCells = new List<Cell>();

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (IsInitialTurn())
                {
                    if (cells[i,j].name == "3,3" || cells[i,j].name == "3,4" || cells[i,j].name == "4,3" || cells[i,j].name == "4,4")
                    {
                        if (!ContainsCard(cells[i,j].transform))
                        {
                            validCells.Add(cells[i,j]);
                        }
                    }
                }
                else
                {
                    if (ContainsCard(cells[i,j].transform))
                    {
                        if (cells[i,j].GetComponentInChildren<Card>().owner.name != PhotonNetwork.player.NickName)
                        {
                            validCells.AddRange(GetValidCellHelper(i, j));
                        }
                    }
                }
            }
        }

        return validCells;
    }

    private List<Cell> GetValidCellHelper(int x, int y)
    {
        List<Cell> validCells = new List<Cell>();
        List<int> cellOffset = new List<int>(){
            -1, 0, 1, -1, 0, 1, -1, 0, 1
        };

        // Visual of what offsets to test:
        //
        //              COLUMN
        //     +-------+-------+-------+
        //     | -1,+1 | +0,+1 | +1,+1 |
        //     +-------+-------+-------+
        // ROW | -1,+0 |  Cell | +1,+0 |
        //     +-------+-------+-------+
        //     | -1,-1 | +0,-1 | +1,-1 |
        //     +-------+-------+-------+
        //

        int column = -1;
        int row = 0;

        for (int i = 0; i < cellOffset.Count; i++)
        {
            if (row >= 3)
            {
                row = 0;
                column++;
            }

            // If tested cell is off boundary
            if (x + column < cells.GetLowerBound(0) || y + cellOffset[i] < cells.GetLowerBound(1) || x + column > cells.GetUpperBound(0) || y + cellOffset[i] > cells.GetUpperBound(1))
            {
                //Debug.Log((x+column) + "," + (y+cellOffset[i]) + " doesn't exist.");
                row++;
                continue;
            }

            // If tested cell already contains card
            if (ContainsCard(cells[x + column, y + cellOffset[i]].transform))
            {
                //Debug.Log((x+column) + "," + (y+cellOffset[i]) + " doesn't contain a card.");
                row++;
                continue;
            }

            // If tested cell can flank opponent
            bool canFlank = CanFlankOpponent(x, y, x + column, y + cellOffset[i]);
            if (!canFlank)
            {
                row++;
                continue;
            }

            //Debug.Log((x+column) + "," + (y+cellOffset[i]) + " is valid.");
            validCells.Add(cells[x + column, y + cellOffset[i]]);
            row++;
        }

        return validCells;
    }

    // x, y = card already on board
    // offsetX, offsetY = potential cell on board based on offset
    private bool CanFlankOpponent(int x, int y, int offsetX, int offsetY)
    {
        Card playerSelectedCard = gameManager.playerList[0].selectedCard;
        Card opponentCard = cells[x,y].GetComponentInChildren<Card>();

        string bestAttack = GetBestAttack(x, y, offsetX, offsetY, playerSelectedCard.attack, opponentCard.attack);
        if (bestAttack == "Player")
        {
            List<Card> potentialMatchedCards = new List<Card>();

            int column = offsetX - x;
            int row = offsetY - y;

            int nextX = offsetX - column;
            int nextY = offsetY - row;
            bool reachedEndOfRow = false;

            int count = 1;

            while (!reachedEndOfRow)
            {
                //Debug.Log("Checking: " + nextX + "," + nextY + " | Original: " + offsetX + "," + offsetY);
                if (nextX < cells.GetLowerBound(0) || nextY < cells.GetLowerBound(1) || nextX > cells.GetUpperBound(0) || nextY > cells.GetUpperBound(1))
                {
                    //Debug.Log("Search out of bounds.");
                    potentialMatchedCards.Clear();
                    reachedEndOfRow = true;
                    break;
                }

                if (!ContainsCard(cells[nextX, nextY].transform))
                {
                    //Debug.Log("Search doesn't have card.");
                    potentialMatchedCards.Clear();
                    reachedEndOfRow = true;
                    break;
                }

                if (cells[nextX, nextY].GetComponentInChildren<Card>().owner.name == playerSelectedCard.owner.name)
                {
                    //Debug.Log("Search found endpoint.");
                    potentialMatchedCards.Add(cells[nextX, nextY].GetComponentInChildren<Card>());
                    reachedEndOfRow = true;
                    break;
                }

                //Debug.Log("Added to potential list. Count: " + count);
                potentialMatchedCards.Add(cells[nextX, nextY].GetComponentInChildren<Card>());

                nextX -= column;
                nextY -= row;
                count++;
            }

            if (potentialMatchedCards.Count > 0)
            {
                if (potentialMatchedCards[potentialMatchedCards.Count - 1].owner.name == playerSelectedCard.owner.name)
                {
                    Debug.Log("PLAYER has higher attack at (" + offsetX + "," + offsetY + ") ");
                    return true;
                }

                Debug.Log("Missing end starting at (" + offsetX + "," + offsetY + ") ");
                return false;
            }

            Debug.Log("No potential matches starting at (" + offsetX + "," + offsetY + ") ");
            return false;
        }
        else if (bestAttack == "Opponent")
        {
            Debug.Log("OPPONENT has higher attack at (" + offsetX + "," + offsetY + ") ");
            return false;
        }

        Debug.Log("Attack EQUAL at (" + offsetX + "," + offsetY + ") ");
        return false;
    }

    public List<Card> GetMatches(Card placedCard, string position)
    {
        List<Card> flippedCards = new List<Card>();
        List<int> cellOffset = new List<int>(){
            -1, 0, 1, -1, 0, 1, -1, 0, 1
        };

        string[] cardPosition = position.Split(","[0]);
        int x = int.Parse(cardPosition[0]);
        int y = int.Parse(cardPosition[1]);

        int column = -1;
        int row = 0;

        for (int i = 0; i < cellOffset.Count; i++)
        {
            if (row >= 3)
            {
                row = 0;
                column++;
            }

            // If tested cell is off boundary
            if (x + column < cells.GetLowerBound(0) || y + cellOffset[i] < cells.GetLowerBound(1) || x + column > cells.GetUpperBound(0) || y + cellOffset[i] > cells.GetUpperBound(1))
            {
                row++;
                continue;
            }

            // If tested cell doesn't have a card
            if (!ContainsCard(cells[x + column, y + cellOffset[i]].transform))
            {
                row++;
                continue;
            }

            // If tested cell is equal to placed card
            if (cells[x + column, y + cellOffset[i]].GetComponentInChildren<Card>().owner.name == placedCard.owner.name)
            {
                row++;
                continue;
            }

            // Add all of opponent's cards
            flippedCards.AddRange(GetMatchesHelper(x, y, x + column, y + cellOffset[i], placedCard, cells[x + column, y + cellOffset[i]].GetComponentInChildren<Card>()));
            row++;
        }

        return flippedCards;
    }

    private List<Card> GetMatchesHelper(int x, int y, int offsetX, int offsetY, Card placedCard, Card cardToCheck)
    {
        List<Card> potentialMatchedCards = new List<Card>();

        string bestAttack = GetBestAttack(offsetX, offsetY, x, y, placedCard.attack, cardToCheck.attack);

        if (bestAttack == "Player")
        {
            int column = offsetX - x;
            int row = offsetY - y;

            int nextX = offsetX;
            int nextY = offsetY;
            bool reachedEndOfRow = false;

            while (!reachedEndOfRow)
            {
                if (nextX < cells.GetLowerBound(0) || nextY < cells.GetLowerBound(1) || nextX > cells.GetUpperBound(0) || nextY > cells.GetUpperBound(1))
                {
                    potentialMatchedCards.Clear();
                    reachedEndOfRow = true;
                    break;
                }

                if (!ContainsCard(cells[nextX, nextY].transform))
                {
                    potentialMatchedCards.Clear();
                    reachedEndOfRow = true;
                    break;
                }

                if (cells[nextX, nextY].GetComponentInChildren<Card>().owner.name == placedCard.owner.name)
                {
                    potentialMatchedCards.Add(cells[nextX, nextY].GetComponentInChildren<Card>());
                    reachedEndOfRow = true;
                    break;
                }

                potentialMatchedCards.Add(cells[nextX, nextY].GetComponentInChildren<Card>());

                nextX += column;
                nextY += row;
            }

            if (potentialMatchedCards.Count > 0)
            {
                if (potentialMatchedCards[potentialMatchedCards.Count - 1].owner.name == placedCard.owner.name)
                {
                    // Include last card in row? Not included for now...
                    potentialMatchedCards.RemoveAt(potentialMatchedCards.Count - 1);
                }
            }
        }

        return potentialMatchedCards;
    }

    /// <summary>
    /// Determines if the game is in its initial turn
    /// </summary>
    /// <returns><c>true</c> if turnCount == 1, else <c>false</c>.</returns>
    private bool IsInitialTurn()
    {
        return gameManager.turnManager.Turn <= 4;
    }

    private string GetBestAttack(int x, int y, int offsetX, int offsetY, Dictionary<string, int> playerCardAttack, Dictionary<string, int> opponentCardAttack)
    {
        int playerAttack = 0;
        int opponentAttack = 0;

        // Bottom-Left
        if (offsetX < x && offsetY < y)
        {
            playerAttack = playerCardAttack["Top"] + playerCardAttack["Right"];
            opponentAttack = opponentCardAttack["Bottom"] + opponentCardAttack["Left"];
        }
        // Middle-Left
        else if (offsetX < x && offsetY == y)
        {
            playerAttack = playerCardAttack["Right"];
            opponentAttack = opponentCardAttack["Left"];
        }
        // Top-Left
        else if (offsetX < x && offsetY > y)
        {
            playerAttack = playerCardAttack["Right"] + playerCardAttack["Bottom"];
            opponentAttack = opponentCardAttack["Top"] + opponentCardAttack["Left"];
        }
        // Bottom-Middle
        else if (offsetX == x && offsetY < y)
        {
            playerAttack = playerCardAttack["Top"];
            opponentAttack = opponentCardAttack["Bottom"];
        }
        // Top-Middle
        else if (offsetX == x && offsetY > y)
        {
            playerAttack = playerCardAttack["Bottom"];
            opponentAttack = opponentCardAttack["Top"];
        }
        // Bottom-Right
        else if (offsetX > x && offsetY < y)
        {
            playerAttack = playerCardAttack["Top"] + playerCardAttack["Left"];
            opponentAttack = opponentCardAttack["Right"] + opponentCardAttack["Bottom"];
        }
        // Middle-Right
        else if (offsetX > x && offsetY == y)
        {
            playerAttack = playerCardAttack["Left"];
            opponentAttack = opponentCardAttack["Right"];
        }
        // Top-Right
        else if (offsetX > x && offsetY > y)
        {
            playerAttack = playerCardAttack["Bottom"] + playerCardAttack["Left"];
            opponentAttack = opponentCardAttack["Top"] + opponentCardAttack["Right"];
        }

        if (playerAttack > opponentAttack)
        {
            return "Player";
        }
        else if (playerAttack < opponentAttack)
        {
            return "Opponent";
        }

        return "None";
    }

    /// <summary>
    /// Checks if a cell already has a Card
    /// </summary>
    /// <returns><c>true</c>, if cell has component Card, else <c>false</c>.</returns>
    /// <param name="cell">Current cell</param>
    private bool ContainsCard(Transform cell)
    {
        if (cell.GetComponentInChildren<Card>())
        {
            return true;
        }
        return false;
    }

    public void ResetValidCells()
    {
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (cells[i,j].gameObject.activeSelf)
                {
                    cells[i,j].GetComponent<Image>().color = Color.white;
                }
            }
        }
    }

    public void ShowCover()
    {
        cover.color = new Color(0, 0, 0, 0.25f);
        cover.GetComponent<Image>().raycastTarget = true;
    }

    public void HideCover()
    {
        cover.color = Color.clear;
        cover.GetComponent<Image>().raycastTarget = false;
    }

    public void AddCoverListener(Player player)
    {
        /*trigger = cover.GetComponent<EventTrigger>();
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener( (data) => { player.UndoSelectCard(); });
        trigger.triggers.Add(entry);*/
    }

    public void RemoveCoverListener()
    {
        //entry.callback.RemoveAllListeners();
        //trigger.triggers.Remove(entry);
    }
}
