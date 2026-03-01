using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject background, text, pencils;
    private int value, guess; // value is 0 if not locked
    private Vector2Int coordinate, box;
    [SerializeField] private Color backgroundDefault, backgroundField, backgroundOrigin, penDefault, penField, penOrigin, pencilDefault, pencilField, pencilOrigin, revealedDefault, revealedField, revealedOrigin;
    private STATE state = STATE.PENCIL;
    private THEME theme = THEME.DEFAULT;
    private Game game;
    private List<int> pencilHistory = new List<int>();

    public enum STATE { PENCIL, PEN, REVEALED }
    public enum THEME { DEFAULT, FIELD, ORIGIN }

    public override string ToString()
    {
        return coordinate.ToString();
    }
    /// <summary>
    /// Returns true if the guess matches the actual value, or if the cell is revealed.
    /// </summary>
    /// <returns></returns
    public bool IsGuessCorrect()
    {
        if (value == 0)
        {
            return false;
        }
        return (state == STATE.REVEALED) || (guess == value);
    }
    public void OnPointerEnter(BaseEventData data)
    {
        //print("OnPointerEnter " + coordinate);
        game.OnCellPointerEnter(this);
    }
    public void OnPointerExit(BaseEventData data)
    {
        //print("OnPointerExit " + coordinate);
        game.OnCellPointerExit(this);
    }
    public void OnPointerClick(BaseEventData data)
    {
        game.OnCellPointerClick(this);
    }
    public void SetGame(Game val)
    {
        game = val;
    }
    public Game GetGame()
    {
        return game;
    }
    /// <summary>
    /// Sets the cell's theme.
    /// </summary>
    public void SetTheme(THEME val)
    {
        theme = val;
        if (val == THEME.DEFAULT)
        {
            background.GetComponent<Image>().color = backgroundDefault;
            SetPencilColor(pencilDefault);
            if (state == STATE.REVEALED)
            {
                // Revealed
                text.GetComponent<TMP_Text>().color = revealedDefault;
            }
            else
            {
                // Guessed (pen color)
                text.GetComponent<TMP_Text>().color = penDefault;
            }
        }
        else if (val == THEME.FIELD)
        {
            background.GetComponent<Image>().color = backgroundField;
            SetPencilColor(pencilField);
            if (state == STATE.REVEALED)
            {
                // Revealed
                text.GetComponent<TMP_Text>().color = revealedField;
            }
            else
            {
                // Guessed (pen)
                text.GetComponent<TMP_Text>().color = penField;
            }
        }
        else if (val == THEME.ORIGIN)
        {
            background.GetComponent<Image>().color = backgroundOrigin;
            SetPencilColor(penOrigin);
            if (state == STATE.REVEALED)
            {
                // Revealed
                text.GetComponent<TMP_Text>().color = revealedOrigin;
            }
            else
            {
                // Guessed (pen)
                text.GetComponent<TMP_Text>().color = penOrigin;
            }
        }
        // Error mode
        else
        {
            background.GetComponent<Image>().color = Color.red;
            text.GetComponent<TMP_Text>().color = Color.white;
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Italic;
        }
    }
    public void SetValue(int val)
    {
        value = val;
    }
    public int GetValue()
    {
        return value;
    }
    public void SetState(STATE val)
    {
        state = val;
        if (state == STATE.REVEALED)
        {
            for (int i = 0; i < 9; i++)
            {
                pencils.transform.GetChild(i).gameObject.SetActive(false);
            }
            guess = value;
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            if (theme == THEME.DEFAULT)
            {
                text.GetComponent<TMP_Text>().color = revealedDefault;
            }
            else if (theme == THEME.FIELD)
            {
                text.GetComponent<TMP_Text>().color = revealedField;
            }
            else if (theme == THEME.ORIGIN)
            {
                text.GetComponent<TMP_Text>().color = revealedOrigin;
            }
            text.GetComponent<TMP_Text>().text = value.ToString();
        }
        else
        {
            guess = 0;
            text.GetComponent<TMP_Text>().text = "";
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Italic;
            switch (theme)
            {
                case THEME.DEFAULT:
                    text.GetComponent<TMP_Text>().color = penDefault;
                    break;
                case THEME.FIELD:
                    text.GetComponent<TMP_Text>().color = penField;
                    break;
                case THEME.ORIGIN:
                    text.GetComponent<TMP_Text>().color = penOrigin;
                    break;
            }
        }
    }
    public void SetPencilColor(Color color)
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject pencilText = pencils.transform.GetChild(i).gameObject;
            pencilText.GetComponent<TMP_Text>().color = color;
        }
    }
    public void UndoPencil()
    {
        if (pencilHistory.Count == 0)
        {
            return;
        }
        TogglePencil(pencilHistory[pencilHistory.Count - 1]);
    }
    public bool GetRevealed()
    {
        return state == STATE.REVEALED;
    }
    public void TogglePencil(int val)
    {
        if ((state == STATE.REVEALED) || val < 0 || val > 9)
        {
            return;
        }
        text.SetActive(false);
        GameObject pencilText = pencils.transform.GetChild(val - 1).gameObject;
        if (pencilText.activeSelf)
        {
            pencilText.SetActive(false);
            pencilHistory.Remove(val);
        }
        else
        {
            state = STATE.PENCIL;
            pencilText.SetActive(true);
            pencilHistory.Add(val);
        }
        
        // remember; when setting the color of the pencil text, use:
        //if (theme == THEME.DEFAULT)
        //{
        //    pencils[val].GetComponent<TMP_Text>().color = pencilDefault;
        //}
        //else if (theme == THEME.FIELD)
        //{
        //    pencils[val].GetComponent<TMP_Text>().color = pencilField;
        //}
        //else if (theme == THEME.ORIGIN)
        //{
        //    pencils[val].GetComponent<TMP_Text>().color = pencilOrigin;
        //}

    }
    public STATE GetState()
    {
        return state;
    }
    public void SetGuess(int val)
    {
        guess = val;
        if (state == STATE.REVEALED)
        {
            return;
        }
        state = STATE.PEN;
        text.SetActive(true);
        for (int i = 0; i < 9; i++)
        {
            pencils.transform.GetChild(i).gameObject.SetActive(false);
        }
        text.GetComponent<TMP_Text>().fontStyle = FontStyles.Italic;
        if (theme == THEME.DEFAULT)
        {
            text.GetComponent<TMP_Text>().color = penDefault;
        }
        else if (theme == THEME.FIELD)
        {
            text.GetComponent<TMP_Text>().color = penField;
        }
        else if (theme == THEME.ORIGIN)
        {
            text.GetComponent<TMP_Text>().color = penOrigin;
        }
        text.GetComponent<TMP_Text>().text = val.ToString();
    }
    public int GetGuess()
    {
        return guess;
    }
    /// <summary>
    /// Clears the cell's guess value.
    /// </summary>
    public void Clear()
    {
        if (state == STATE.REVEALED)
        {
            return;
        }
        guess = 0;
        text.GetComponent<TMP_Text>().text = "";
        for (int i = 0; i < 9; i++)
        {
            pencils.transform.GetChild(i).gameObject.SetActive(false);
        }
        pencilHistory.Clear();
    }
    public void SetCoordinate(Vector2Int val)
    {
        coordinate = val;
    }
    public Vector2Int GetCoordinate()
    {
        return coordinate;
    }
    public void SetBox(Vector2Int val)
    {
        box = val;
    }
    public Vector2Int GetBox()
    {
        return box;
    }
}
