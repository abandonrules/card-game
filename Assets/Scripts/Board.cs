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
                            cells[i,j].GetComponent<Image>().color = Color.green;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines if the game is in its initial turn
    /// </summary>
    /// <returns><c>true</c> if turnCount == 1, else <c>false</c>.</returns>
    bool IsInitialTurn()
    {
        return gameManager.turnManager.Turn <= 4;
    }

    /// <summary>
    /// Checks if a cell already has a Card
    /// </summary>
    /// <returns><c>true</c>, if cell has component Card, else <c>false</c>.</returns>
    /// <param name="cell">Current cell</param>
    bool ContainsCard(Transform cell)
    {
        if (cell.GetComponentInChildren<Card>())
        {
            return true;
        }
        return false;
    }

    public void HideValidCells()
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
