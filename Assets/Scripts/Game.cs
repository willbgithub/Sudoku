using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private const int SIZE = 9;
    private Cell[,] cells = new Cell[SIZE,SIZE];
    [SerializeField] private GameObject cellPrefab, cellBoard;
    private Cell selection;
    private bool selected = false;
    void Start()
    {
        AssignArray();
        GenerateNewBoard();
        ChangeSelection(cells[4, 0]);
    }
    private void ChangeSelection(Cell cell, bool voluntary = false)
    {
        if (voluntary)
        {
            selected = true;
        }
        selection = cell;
        List<Cell> targetCells = GetTargetCells(cell);
        for (int row = 0; row<SIZE; row++)
        {
            for (int column = 0; column<SIZE; column++)
            {
                if (targetCells.Contains(cells[column, row]))
                {
                    cells[column, row].SetColor(1);
                }
                else
                {
                    cells[column, row].SetColor(0);
                }
            }
        }
        cell.SetColor(2);
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
                        Cell cell = box.GetChild(row * 3 + column).GetComponent<Cell>();
                        cell.SetCoordinate(new Vector2Int(column + boxColumn * 3, row + boxRow * 3));
                        cell.SetBox(new Vector2Int(boxColumn, boxRow));

                        cells[column + boxColumn * 3, row + boxRow * 3] = cell;
                    }
                }
            }
        }
    }
    private void GenerateNewBoard()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
                cells[column, row].SetValue(0);
                cells[column, row].SetRevealed(false);
            }
        }
        NextStep();
    }
    private bool IsValidCoordinate(Vector2Int coordinate)
    {
        return (coordinate.x >= 0 && coordinate.x < SIZE && coordinate.y >= 0 && coordinate.y < SIZE);
    }
    // Returns false if the step results in a contradiction.
    private bool NextStep()
    {
        if (!BoardNotFinished())
        {
            return true;
        }
        Cell target = GetMostSolved();
        List<int> potentialAnswers = GetPotentialAnswers(target);
        for (int i = 0; i < potentialAnswers.Count; i++)
        {
            target.SetValue(potentialAnswers[i]);
            if (NextStep())
            {
                return true;
            }
        }
        // This step has run out of valid numbers, meaning there is a contradiction
        target.SetValue(0);
        return false;
    }
    private bool BoardNotFinished()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetValue() == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
    // Returns the cell with the lowest amount of potential answers.
    private Cell GetMostSolved()
    {
        List<Cell> returnList = new List<Cell>();
        int leastAnswers = 9;
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetValue() != 0)
                {
                    continue;
                }
                int potentialAnswers = CountPotentialAnswers(cells[column, row]);
                if (potentialAnswers >= leastAnswers)
                {
                    continue;
                }
                leastAnswers = potentialAnswers;
            }
        }
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetValue() != 0)
                {
                    continue;
                }
                int potentialAnswers = CountPotentialAnswers(cells[column, row]);
                if (potentialAnswers > leastAnswers)
                {
                    continue;
                }
                returnList.Add(cells[column, row]);
            }
        }
        return returnList[Random.Range(0, returnList.Count)];
    }
    private List<int> GetPotentialAnswers(Cell cell)
    {
        if (cell.GetValue() != 0)
        {
            return new List<int>();
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetCells(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            if (!answers.Contains(targetCells[i].GetValue()))
            {
                continue;
            }
            answers.Remove(targetCells[i].GetValue());
        }
        return answers;
    }
    private int CountPotentialAnswers(Cell cell)
    {
        if (cell.GetValue() != 0)
        {
            return 0;
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetCells(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            if (!answers.Contains(targetCells[i].GetValue()))
            {
                continue;
            }
            answers.Remove(targetCells[i].GetValue());
        }
        return answers.Count;
    }
    private List<Cell> GetTargetCells(Cell cell)
    {
        List<Cell> targets = new List<Cell>();
        targets = AddNew(targets, GetTargetColumn(cell));
        targets = AddNew(targets, GetTargetRow(cell));
        targets = AddNew(targets, GetTargetBox(cell));
        return targets;
    }
    private List<Cell> AddNew(List<Cell> mainList, List<Cell> list2)
    {
        for(int i = 0; i < list2.Count; i++)
        {
            if (mainList.Contains(list2[i]))
            {
                continue;
            }
            mainList.Add(list2[i]);
        }
        return mainList;
    }
    private List<Cell> GetTargetColumn(Cell cell)
    {
        List<Cell> targets = new List<Cell>();
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetCoordinate().x != cell.GetCoordinate().x)
                {
                    continue;
                }
                targets.Add(cells[column, row]);
            }
        }
        targets.Remove(cell);
        return targets;
    }
    private List<Cell> GetTargetRow(Cell cell)
    {
        List<Cell> targets = new List<Cell>();
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetCoordinate().y != cell.GetCoordinate().y)
                {
                    continue;
                }
                targets.Add(cells[column, row]);
            }
        }
        targets.Remove(cell);
        return targets;
    }
    private List<Cell> GetTargetBox(Cell cell)
    {
        List<Cell> targets = new List<Cell>();
        for (int row = 0; row < SIZE; row ++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetBox() != cell.GetBox())
                {
                    continue;
                }
                targets.Add(cells[column, row]);
            }
        }
        targets.Remove(cell);
        return targets;
    }
}
