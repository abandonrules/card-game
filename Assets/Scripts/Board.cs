using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    // Prefab object to instantiate
    public GameObject cellPrefab;
    // Reference to all cell objects on the board
    public List<Transform> cells;
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
    public int width;
    public int height;

    private Image cover;
    private EventTrigger trigger;
    private EventTrigger.Entry entry;

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

        GameObject coverGO = GameObject.Find("Board Cover");
        cover = coverGO.GetComponent<Image>();
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
                cells.Add(cell.transform);
                cell.GetComponent<Cell>().gameManager = FindObjectOfType<GameManager>();
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
        foreach(Transform cell in cells)
        {
            if (IsInitialTurn())
            {
                // Fix: Not hardcoded?
                if (cell.name == "3,3" || cell.name == "3,4" || cell.name == "4,3" || cell.name == "4,4")
                {
                    if (!ContainsCard(cell))
                    {
                        cell.GetComponent<Image>().color = Color.green;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines if the game is in its Initial Turn
    /// </summary>
    /// <returns><c>true</c> if turnCount == 0, else <c>false</c>.</returns>
    bool IsInitialTurn()
    {
        if(gameManager.turnCount == 0)
        {
            return true;
        }
        return false;
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
        foreach(Transform cell in cells)
        {
            if (cell.gameObject.activeSelf)
            {
                cell.GetComponent<Image>().color = Color.white;
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
        trigger = cover.GetComponent<EventTrigger>();
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener( (data) => { player.UndoSelectCard(); });
        trigger.triggers.Add(entry);
    }

    public void RemoveCoverListener()
    {
        entry.callback.RemoveAllListeners();
        trigger.triggers.Remove(entry);
    }
}
