using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class Game : MonoBehaviour
{
    private const int SIZE = 9;
    private Cell[,] cells = new Cell[SIZE,SIZE];
    [SerializeField] private GameObject cellPrefab, cellBoard;
    private Cell selection;
    private bool selected = false, solving = false;
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
        List<Cell> targetCells = GetTargetField(cell);
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
        // Reset all cells
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
                cells[column, row].SetValue(0);
                cells[column, row].SetRevealed(false);
            }
        }
        // Starts the generation algorithm.
        NextStep();
        // Reveal all cells
        List<Cell> revealed = new List<Cell>();
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].SetRevealed(true);
                revealed.Add(cells[column, row]);
            }
        }
        // Unreveal until board is just one step away from being unsolvable
        while (BoardIsSolvable())
        {
            Cell cell = revealed[Random.Range(0, revealed.Count-1)];
            cell.SetRevealed(false);
            revealed.Remove(cell);
        }
        }
    private bool BoardIsSolvable()
    {
        solving = true;

        while(IdleCellExists())
        {
            SolveCell();
        }

        solving = false;

        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
            }
        }
        return true;
    }
    // returns true if there is any cell that is not solved or not guessed
    private bool IdleCellExists()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (!cells[column, row].GetRevealed() && cells[column, row].GetGuess() == 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
    // Finds the first unsolved cell that can be solved. If no cells are found, returns false.
    // Solves the cell by putting the correct guess in.
    private bool SolveCell()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                Cell cell = cells[column, row];
                // Cell is already solved
                if (cell.GetRevealed())
                {
                    return true;
                }
                List<int> potentialAnswers = GetPotentialAnswers(cell, false);
                // Only has one potential answer
                if (potentialAnswers.Count == 1)
                {
                    cell.SetGuess(potentialAnswers[0]);
                    return true;
                }
                // Only one in target field with a certain potential answer
                List<Cell> targetField = GetTargetField(cell);
                for (int i = 0; i < targetField.Count; i++)
                {
                    List<int> targetPotentialAnswers = GetPotentialAnswers(targetField[i], false);
                    for(int j = 0; j < targetPotentialAnswers.Count; j++)
                    {
                        if (potentialAnswers.Contains(targetPotentialAnswers[j]))
                        {
                            potentialAnswers.Remove(targetPotentialAnswers[j]);
                        }
                    }
                }
                if (potentialAnswers.Count == 1)
                {
                    cell.SetGuess(potentialAnswers[0]);
                    return true;
                }
            }
        }
        return false;
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
    // Finds and returns the cell with the lowest amount of potential answers.
    private Cell GetMostSolved(bool omniscient = true)
    {
        List<Cell> returnList = new List<Cell>();
        // Assume the lowest number of potential answers across the board is 9
        int leastAnswers = 9;
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                Cell cell = cells[column, row];

                // if cell is already solved, abandon it
                if (omniscient && cell.GetValue() != 0 || !omniscient && solving && cell.GetGuess() != 0)
                {
                    continue;
                }

                int potentialAnswers = CountPotentialAnswers(cell, omniscient);
                if (potentialAnswers >= leastAnswers)
                {
                    continue;
                }
                
                // If the potential answers from this cell are lower than leastAnswers, update it
                leastAnswers = potentialAnswers;
            }
        }
        print(leastAnswers);
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (cells[column, row].GetValue() != 0)
                {
                    continue;
                }
                int potentialAnswers = CountPotentialAnswers(cells[column, row], omniscient);
                if (potentialAnswers > leastAnswers)
                {
                    continue;
                }
                returnList.Add(cells[column, row]);
            }
        }
        return returnList[Random.Range(0, returnList.Count)];
    }
    private List<int> GetPotentialAnswers(Cell cell, bool omniscient = true)
    {
        // Cell has a value and game knows it; 0 potential answers
        if (cell.GetValue() != 0 && (omniscient || cell.GetRevealed()))
        {
            return new List<int>();
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetField(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            if(!omniscient && !cell.GetRevealed() || !answers.Contains(targetCells[i].GetValue()))
            {
                continue;
            }
            // If the target cell has a known value in answers, remove it from answers
            answers.Remove(targetCells[i].GetValue());
        }
        return answers;
    }
    // Counts number of answers a cell could potentially have without breaking a rule
    private int CountPotentialAnswers(Cell cell, bool omniscient = true)
    {
        // Cell already has a value and game knows it
        if (cell.GetValue() != 0 && omniscient)
        {
            return 0;
        }
        if (cell.GetRevealed() && !omniscient && solving)
        {
            return 0;
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetField(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            // If the target cell has a known value in answers, remove it from answers
            if ( !(omniscient && answers.Contains(targetCells[i].GetValue())) && !(!omniscient && targetCells[i].GetRevealed() && solving && answers.Contains(targetCells[i].GetValue())) )
            {
                continue;
            }
            answers.Remove(targetCells[i].GetValue());
        }
        return answers.Count;
    }
    // gets all cells in the row, column, and box of the specified cell
    private List<Cell> GetTargetField(Cell cell)
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
