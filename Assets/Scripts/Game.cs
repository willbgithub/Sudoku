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
    private bool selected = false;
    private bool solving = false; // used by program when trying to see if a board is solvable without cheating
    [SerializeField] private bool debug = false;
    void Start()
    {
        AssignArray();
        GenerateNewBoard();
        //print(BoardIsSolvable());
    }
    public void OnCellPointerEnter(Cell cell)
    {
        if (selected)
        {
            return;
        }
        ChangeSelection(cell);
    }
    public void OnCellPointerExit(Cell cell)
    {
        //if (selected)
        //{
        //    return;
        //}
        //EmptySelection();
    }
    public void OnCellPointerClick(Cell cell)
    {
        if (selected && selection == cell)
        {
            EmptySelection(true);
            return;
        }
        ChangeSelection(cell, true);

        if (debug)
        {
            if (cell.GetRevealed())
            {

            }
            else
            {
                List<int> potentialAnswers = GetPotentialAnswers(cell, false, true);
                string message = "";
                for (int i = 0; i < potentialAnswers.Count-1; i++)
                {
                    message += potentialAnswers[i] + ", ";
                }
                if (potentialAnswers.Count > 0)
                {
                    message += potentialAnswers[potentialAnswers.Count - 1];
                }
                print(message);
            }
        }
    }

    // Returns true if the board can be solved with only ONE solution
    private bool BoardIsSolvable()
    {
        print("BoardIsSolvable() called");
        solving = true;
        // Reset guess values of all cells
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
            }
        }
        bool solved = true;
        // Attempt to solve every idle cell
        while (IdleCellExists() && solved)
        {
            solved = false;
            // Attempt to solve a cell
            for (int row = 0; row < SIZE; row++)
            {
                for (int column = 0; column < SIZE; column++)
                {
                    // If the cell already has a guess or is revealed, move past it
                    Cell cell = cells[column, row];
                    print("NEW CELL: " + new Vector2Int(column, row));
                    if (cell.GetGuess() != 0 || cell.GetRevealed())
                    {
                        if (cell.GetGuess() != 0)
                        {
                            print("cell.GetGuess() = " + cell.GetGuess());
                        }
                        if (cell.GetRevealed())
                        {
                            print("cell.GetRevealed() = " + cell.GetRevealed());
                        }
                        continue;
                    }
                    List<int> potentialAnswers = GetPotentialAnswers(cell, false);
                    // sanity check
                    if (potentialAnswers.Count == 0)
                    {
                        print("THIS SHOULD NOT BE POSSIBLE!!!!");
                        cell.SetColor(4);
                        return false;
                    }
                    // Cell only has one potential answer: SOLVE!
                    if (potentialAnswers.Count == 1)
                    {
                        print("potentialAnswers.Count == 1");
                        print("Solved with " + potentialAnswers[0]);
                        cell.SetGuess(potentialAnswers[0]);
                        solved = true;
                    }
                    // Multiple potential answers; see if it has a unique one in its target field
                    else if (potentialAnswers.Count > 1)
                    {
                        List<int> targetAnswers = GetTargetFieldAnswers(cell);
                        for (int i = 0; i < potentialAnswers.Count; i++)
                        {
                            if (targetAnswers.Contains(potentialAnswers[i]))
                            {
                                continue;
                            }
                            // Answer is unique! SOLVE!
                            print("Cell is the only one with this answer.");
                            print("Solved with " + potentialAnswers[i]);
                            cell.SetGuess(potentialAnswers[i]);
                            solved = true;
                        }
                    }
                }
            }
        }
        solving = false;
        // Reset guess values of all cells, again
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
            }
        }
        if (!solved)
        {
            print("Last run didn't solve any cells. Unsolvable board");
            // Last run did not solve any cells, so the board is unsolvable
            return false;
        }
        print("Ran out of idle cells. Solvable board.");
        return true;
    }
    // Returns all the possible answers in the entire target field of a given cell
    private List<int> GetTargetFieldAnswers(Cell cell, bool omniscient = true)
    {
        List<int> potentialAnswers = new List<int>();
        List<Cell> targetField = GetTargetField(cell);
        for (int i = 0; i < targetField.Count; i++)
        {
            List<int> targetAnswers = GetPotentialAnswers(targetField[i], omniscient);
            for (int j =  0; j < targetAnswers.Count; j++)
            {
                if (potentialAnswers.Contains(targetAnswers[j]))
                {
                    continue;
                }
                potentialAnswers.Add(targetAnswers[j]);
            }
        }
        return potentialAnswers;
    }
    // returns true if there is any cell that is not solved or not guessed
    private bool IdleCellExists()
    {
        print("IdleCellExists() called");
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                // if a cell is not solved or not guessed, then there is an idle cell. return true
                if (!cells[column, row].GetRevealed() && cells[column, row].GetGuess() == 0)
                {
                    print("Found idle cell: " + new Vector2Int(column, row));
                    return true;
                }
            }
        }
        print("No idle cells found.");
        return false;
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
    private void EmptySelection(bool voluntary = false)
    {
        if (voluntary)
        {
            selected = false;
        }
        selection = null;
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].SetColor(0);
            }
        }
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
                        cell.SetGame(this);
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
                Cell cell = cells[column, row];
                cell.Clear();
                cell.SetValue(0);
                cell.SetRevealed(false);
                
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
                Cell cell = cells[column, row];
                cell.SetRevealed(true);
                revealed.Add(cell);
            }
        }
        // Unreveal until board is just one step away from being unsolvable
        while (BoardIsSolvable())
        {
            Cell cell = revealed[Random.Range(0, revealed.Count - 1)];
            cell.SetRevealed(false);
            revealed.Remove(cell);
        }
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
    private List<int> GetPotentialAnswers(Cell cell, bool omniscient = true, bool debug2 = false)
    {
        if (debug2)
        {
            print("GetPotentialAnswers(" + cell.GetCoordinate() + ", omniscient = " + omniscient + ") called");
        }
        // Cell has a value and game knows it; 0 potential answers
        if (cell.GetValue() != 0 && (omniscient || cell.GetRevealed()))
        {
            return new List<int>();
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetField(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            Cell targetCell = targetCells[i];
            // If the program knows that all guesses are equal to true values, use the guess value
            if(solving && answers.Contains(targetCell.GetGuess()))
            {
                //print("solving = " + solving + " && answers.Contains(" + targetCell.GetGuess() + ") == true");
                answers.Remove(targetCell.GetGuess());
            }
            // If the function is omniscient, use the actual value
            else if (omniscient && answers.Contains(targetCell.GetValue()))
            {
                //print("omniscient = " + omniscient + " && answers.Contains(" + targetCell.GetValue() + ") == true");
                answers.Remove(targetCell.GetValue());
            }
            // If the function isn't omniscient, but the cell is revealed, use the actual value
            else if (targetCell.GetRevealed() && answers.Contains(targetCell.GetValue()))
            {
                answers.Remove(targetCell.GetValue());
            }
            else
            {
                //print("solving = " + solving + " && answers.Contains(" + targetCell.GetGuess() + ") == " + (solving && answers.Contains(targetCell.GetGuess())));
                if (debug2)
                {
                    print("omniscient = " + omniscient + " && answers.Contains(" + targetCell.GetValue() + ") == " + (omniscient && answers.Contains(targetCell.GetValue())));
                    print("targetCell.GetRevealed() == " + targetCell.GetRevealed() + " && answers.Contains(" + targetCell.GetValue() + ") == " + (targetCell.GetRevealed() && answers.Contains(targetCell.GetValue())));
                }
            }
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
