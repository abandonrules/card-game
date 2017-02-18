using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

    // Prefab object to instantiate
    public GameObject cellPrefab;
    // Reference to all cell objects on the board
    public List<GameObject> cells;
    // Enumerator to determine size of board
    public enum Matrix
    {
        ThreeByThree,
        FourByFour,
        FiveByFive,
        SixBySix
    };
    public Matrix matrix;
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
            default:
                width = 5;
                height = 5;
                break;
        };
    }

    /// <summary>
    /// Create the board, instantiating cells for each position of the board
    /// </summary>
	public void Create()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject cell = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
                cells.Add(cell);
                cell.name = i + "," + j;
                cell.GetComponent<RectTransform>().localPosition = new Vector2(i * 50, j * 50);
            }
        }
    }
}
