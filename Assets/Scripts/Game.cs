using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private const int SIZE = 9;
    private Cell[,] cells = new Cell[SIZE,SIZE];
    [SerializeField] private GameObject cellPrefab, cellBoard;
    void Start()
    {
        AssignArray();
        GenerateBoard();
    }
    private void AssignArray()
    {
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxColumn = 0; boxColumn < 3; boxColumn++)
            {
                Transform box = cellBoard.transform.GetChild(boxRow*3 + boxColumn);
                for (int row = 0; row < 3; row++)
                {
                    for (int column = 0; column < 3; column++)
                    {
                        cells[column + boxColumn * 3, row + boxRow * 3] = box.GetChild(row*3 + column).GetComponent<Cell>();
                        cells[column + boxColumn * 3, row + boxRow * 3].SetCoordinate(new Vector2Int(column + boxColumn * 3, row + boxRow * 3));
                        cells[column + boxColumn * 3, row + boxRow * 3].SetValue(5);
                    }
                }
            }
        }
    }
    private void GenerateBoard()
    {

    }
}
