using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    // Prefab object to instantiate
    public GameObject cellPrefab;
    // Reference to all cell objects on the board
    public List<GameObject> cells;
    // Reference to all Cards on the board
    public List<Card> cardList;
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
                cells.Add(cell);
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
}
