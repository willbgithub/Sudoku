using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.U2D.IK;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class Game : MonoBehaviour
{
    private const int SIZE = 9;
    private Cell[,] cells = new Cell[SIZE,SIZE];
    [SerializeField] private GameObject cellPrefab, cellBoard;
    private Cell selection;
    private bool selected = false;

    //[SerializeField] private bool debug = false;

    public void LETERRIP()
    {
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
        if (!BoardIsSolvable())
        {
            print("HOW!!!!!!");
            return;
        }
        Cell selectedCell = null;
        while (BoardIsSolvable())
        {
            selectedCell = revealed[Random.Range(0, revealed.Count-1)];
            selectedCell.SetRevealed(false);
            selectedCell.Clear();
            revealed.Remove(selectedCell);
        }
        selectedCell.SetRevealed(true);
        print("WE DID TI");
    }
    public void DebugIdleCellExists()
    {
        print(IdleCellExists());
    }
    public void DebugBoardIsSolvable()
    {
        print(BoardIsSolvable());
    }
    public void DebugSolveCell()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        print(SolveCell(selection));
    }
    public void DebugGetPotentialAnswers()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        PrintList(GetPotentialAnswers(selection, false));
    }
    public void DebugGetPotentialAnswersOmniscient()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        PrintList(GetPotentialAnswers(selection, true));
    }
    public void DebugIsIdleCell()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        print(IsIdleCell(selection));
    }
    public void DebugGetTargetFieldAnswers()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        PrintList(GetTargetFieldAnswers(selection, false));
    }
    public void DebugGetTargetFieldAnswersOmniscient()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        PrintList(GetTargetFieldAnswers(selection, true));
    }
    public void DebugClearGuess()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        selection.Clear();
    }
    public void DebugToggleReveal()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        selection.SetRevealed(!selection.GetRevealed());
    }
    public void DebugIsGuessCorrect()
    {
        if (!selection)
        {
            print("No cell selected!");
            return;
        }
        print(selection.IsGuessCorrect());
    }


    // ---------SETUP---------
    void Start()
    {
        AssignArray();
        GenerateNewBoard();
    }
    /// <summary>
    /// Sets up the cell board by assigning each cell to the array and initializing their data.
    /// </summary>
    private void AssignArray()
    {
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxColumn = 0; boxColumn < 3; boxColumn++)
            {
                Transform box = cellBoard.transform.GetChild(boxRow * 3 + boxColumn);
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
    /// <summary>
    /// Empties the board and creates a new one.
    /// </summary>
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
        
    }
    // ---------INTERMEDIATE---------
    /// <summary>
    /// Returns true if the board is solvable with exactly one solution. Will reset all guessed values to do so.
    /// </summary>
    /// <returns></returns>
    private bool BoardIsSolvable()
    {
        print("BoardIsSolvable() called");
        bool solvable = true;
        int counter = 0;
        while (IdleCellExists() && solvable && counter < 150)
        {
            solvable = false;
            for (int row = 0; row < SIZE; row++)
            {
                for (int column = 0; column < SIZE; column++)
                {
                    bool result = SolveCell(cells[column, row]);
                    solvable = (solvable || result);
                }
            }
            counter++;
        }
        // Reset all guessed values
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                cells[column, row].Clear();
            }
        }
        if (counter >= 150)
        {
            print("anti-crash activated! calling SolveCell(cells[4, 4])");
            print(SolveCell(cells[4, 4]));
        }
        return solvable;
    }
    /// <summary>
    /// Attempt to solve the given cell. Returns true if the cell was successfully solved. Returns false if unable to solve, or cell was already solved.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private bool SolveCell(Cell cell)
    {
        if (!IsIdleCell(cell))
        {
            // Cell is already revealed. Can't solve it.
            return false;
        }
        List<int> potentialAnswers = GetPotentialAnswers(cell, false);
        if (potentialAnswers.Count == 0)
        {
            // UNEXPECTED ERROR
            print("THIS SHOULD NOT BE POSSIBLE!");
            return false;
        }
        // If there is only one potential answer, solve it
        if (potentialAnswers.Count == 1)
        {
            print("Cell solved. Only one potential answer.");
            cell.SetGuess(potentialAnswers[0]);
            return true;
        }
        // If there are multiple potential answers, check if it has a unique one in its field
        if (potentialAnswers.Count > 1)
        {
            List<int> targetFieldAnswers = GetTargetFieldAnswers(cell, false);
            List<int> uniqueAnswers = new List<int>();
            for (int i = 0; i < potentialAnswers.Count; i++)
            {
                if (targetFieldAnswers.Contains(potentialAnswers[i]))
                {
                    continue;
                }
                uniqueAnswers.Add(potentialAnswers[i]);
            }
            if (uniqueAnswers.Count == 0)
            {
                // No unique answers. Not enough information to solve cell.
                //print("Not enough information to solve this cell.");
                //print("Potential answers of " + cell + ": ");
                //PrintList(potentialAnswers);
                //print("Target field answers of " + cell + ": ");
                //PrintList(targetFieldAnswers);
                
                return false;
            }
            if (uniqueAnswers.Count == 1)
            {
                // Unique answer found. Solving.
                print("Cell solved. Unique potential answer.");
                cell.SetGuess(uniqueAnswers[0]);
                return true;
            }
            if (uniqueAnswers.Count > 1)
            {
                // UNEXPECTED ERROR
                print("by definition of unique answer how can there be more than 1 you liar");
                return false;
            }
        }
        // UNEXPECTED ERROR
        print("HOW WAS THIS ACCESSED");
        return false;
    }
    private void PrintList(List<int> list)
    {
        string message = "";
        if (list.Count == 0)
        {
            print("Empty list");
            return;
        }
        for (int i = 0; i < list.Count-1; i++)
        {
            message += list[i] + ", ";
        }
        message += list[list.Count - 1];
        print(message);
    }
    
    /// <summary>
    /// Returns true if there is any cell that isn't revealed and isn't guessed.
    /// </summary>
    /// <returns></returns>
    private bool IdleCellExists()
    {
        print("IdleCellExists() called");
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                // if a cell is not solved or not guessed, then there is an idle cell. return true
                if (IsIdleCell(cells[column, row]))
                {
                    print("Found idle cell: " + new Vector2Int(column, row));
                    return true;
                }
            }
        }
        print("No idle cells found.");
        return false;
    }
    /// <summary>
    /// Returns true if the cell isn't revealed and isn't guessed.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private bool IsIdleCell(Cell cell)
    {
        return (!cell.GetRevealed() && !cell.IsGuessCorrect());
    }
    /// <summary>
    /// Returns true if the coordinate is within the bounds of the board ((0,0) to (8,8))
    /// </summary>
    /// <param name="coordinate"></param>
    /// <returns></returns>
    private bool IsValidCoordinate(Vector2Int coordinate)
    {
        return (coordinate.x >= 0 && coordinate.x < SIZE && coordinate.y >= 0 && coordinate.y < SIZE);
    }
    /// <summary>
    /// Takes the next step in the generation algorithm.
    /// </summary>
    /// <returns>True, if the step didn't result in a contradiction.</returns>
    private bool NextStep()
    {
        if (!BoardNotFinished())
        {
            return true;
        }
        Cell target = GetMostSolved();
        List<int> potentialAnswers = GetPotentialAnswers(target, true);
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
    /// <summary>
    /// Returns true if there are any cells with no value assigned.
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Finds and returns the cell with the lowest amount of potential answers.
    /// </summary>
    /// <param name="omniscient"></param>
    /// <returns></returns>
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
                if (omniscient && cell.GetValue() != 0 || !omniscient && cell.IsGuessCorrect())
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
    /// <summary>
    /// Gets the potential answers a cell can have without contradicting another.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="omniscient"></param>
    /// <param name="debug2"></param>
    /// <returns></returns>
    private List<int> GetPotentialAnswers(Cell cell, bool omniscient, bool debug2 = false)
    {
        //print("GetPotentialAnswers(" + cell + ", omniscient=" + omniscient + ") called");
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
            // If the program knows that the guess is correct, use it
            if(targetCell.IsGuessCorrect() && answers.Contains(targetCell.GetValue()))
            {
                answers.Remove(targetCell.GetValue());
            }
            // If the function is omniscient, use the actual value
            else if (omniscient && answers.Contains(targetCell.GetValue()))
            {
                answers.Remove(targetCell.GetValue());
            }
            else
            {
                //print(targetCell + " has a guess of " + targetCell.GetGuess() + " and an actual value of " + targetCell.GetValue() + ". Current answers: ");
                //PrintList(answers);
            }
        }
        return answers;
    }
    /// <summary>
    /// Counts number of answers a cell could potentially have without breaking a rule
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="omniscient"></param>
    /// <returns></returns>
    private int CountPotentialAnswers(Cell cell, bool omniscient)
    {
        // Cell already has a value and game knows it
        if (cell.GetValue() != 0 && omniscient)
        {
            return 0;
        }
        if (cell.GetRevealed() && !omniscient && cell.IsGuessCorrect())
        {
            return 0;
        }
        List<int> answers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<Cell> targetCells = GetTargetField(cell);
        for (int i = 0; i < targetCells.Count; i++)
        {
            // If the target cell has a known value in answers, remove it from answers
            if ( !(omniscient && answers.Contains(targetCells[i].GetValue())) && !(!omniscient && targetCells[i].GetRevealed() && cell.IsGuessCorrect() && answers.Contains(targetCells[i].GetValue())) )
            {
                continue;
            }
            answers.Remove(targetCells[i].GetValue());
        }
        return answers.Count;
    }
    /// <summary>
    /// Returns all the possible answers in the entire target field of a given cell
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="omniscient"></param>
    /// <returns></returns>
    private List<int> GetTargetFieldAnswers(Cell cell, bool omniscient)
    {
        List<int> potentialAnswers = new List<int>();
        List<Cell> targetField = GetTargetField(cell);
        for (int i = 0; i < targetField.Count; i++)
        {
            List<int> targetAnswers = GetPotentialAnswers(targetField[i], omniscient);
            for (int j = 0; j < targetAnswers.Count; j++)
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
    /// <summary>
    /// Gets all cells in the row, column, and box of the specified cell. Does not include the origin cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private List<Cell> GetTargetField(Cell cell)
    {
        List<Cell> targets = new List<Cell>();
        targets = AddNew(targets, GetTargetColumn(cell));
        targets = AddNew(targets, GetTargetRow(cell));
        targets = AddNew(targets, GetTargetBox(cell));
        return targets;
    }
    /// <summary>
    /// Combines two lists without adding duplicate data.
    /// </summary>
    /// <param name="mainList"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the target column of a specified cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the target row of a specified cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Gets the target box of a specified cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
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


    // ----------CONTROLS----------
    private void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            OnLeft();
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            OnRight();
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            OnUp();
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            OnDown();
        }
    }
    public void OnLeft()
    {
        if (!selected)
        {
            return;
        }
        Vector2Int newCoordinate = selection.GetCoordinate() + new Vector2Int(-1, 0);
        if (!IsValidCoordinate(newCoordinate))
        {
            return;
        }
        ChangeSelection(cells[newCoordinate.x, newCoordinate.y]);
    }
    public void OnRight()
    {
        if (!selected)
        {
            return;
        }
        Vector2Int newCoordinate = selection.GetCoordinate() + new Vector2Int(1, 0);
        if (!IsValidCoordinate(newCoordinate))
        {
            return;
        }
        ChangeSelection(cells[newCoordinate.x, newCoordinate.y]);
    }
    public void OnUp()
    {
        if (!selected)
        {
            return;
        }
        Vector2Int newCoordinate = selection.GetCoordinate() + new Vector2Int(0, -1);
        if (!IsValidCoordinate(newCoordinate))
        {
            return;
        }
        ChangeSelection(cells[newCoordinate.x, newCoordinate.y]);
    }
    public void OnDown()
    {
        if (!selected)
        {
            return;
        }
        Vector2Int newCoordinate = selection.GetCoordinate() + new Vector2Int(0, 1);
        if (!IsValidCoordinate(newCoordinate))
        {
            return;
        }
        ChangeSelection(cells[newCoordinate.x, newCoordinate.y]);
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

    }
    public void OnCellPointerClick(Cell cell)
    {

        if (selected && selection == cell)
        {
            EmptySelection(true);
            return;
        }
        ChangeSelection(cell, true);
    }
    /// <summary>
    /// Changes the selected cell.
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="voluntary"></param>
    private void ChangeSelection(Cell cell, bool voluntary = false)
    {
        if (voluntary)
        {
            selected = true;
        }
        selection = cell;
        List<Cell> targetCells = GetTargetField(cell);
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                if (targetCells.Contains(cells[column, row]))
                {
                    cells[column, row].SetTheme(Cell.THEME.FIELD);
                }
                else
                {
                    cells[column, row].SetTheme(0);
                }
            }
        }
        cell.SetTheme(Cell.THEME.ORIGIN);
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
                cells[column, row].SetTheme(Cell.THEME.DEFAULT);
            }
        }
    }
}
