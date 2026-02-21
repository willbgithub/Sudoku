using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject background, text;
    private int value, guess; // value is 0 if not locked
    private Vector2Int coordinate, box;
    [SerializeField] private Color backgroundDefault, backgroundField, backgroundOrigin, penDefault, penField, penOrigin, pencilDefault, pencilField, pencilOrigin, revealedDefault, revealedField, revealedOrigin;
    private bool revealed = false;
    private THEME theme = THEME.DEFAULT;
    private Game game;

    public enum THEME { DEFAULT, FIELD, ORIGIN}

    public override string ToString()
    {
        return coordinate.ToString();
    }
    /// <summary>
    /// Returns true if the guess matches the actual value, or if the cell is revealed.
    /// </summary>
    /// <returns></returns>
    public bool IsGuessCorrect()
    {
        if (value == 0)
        {
            return false;
        }
        return revealed || (guess == value);
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
            if (revealed)
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
            if (revealed)
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
            if (revealed)
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
    public void SetRevealed(bool val)
    {
        revealed = val;
        if (revealed)
        {
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
            if (guess == 0)
            {
                text.GetComponent<TMP_Text>().text = "";
            }
            else
            {
                text.GetComponent<TMP_Text>().text = guess.ToString();
            }
        }
    }
    public void TogglePencil(int val)
    {
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
    public bool GetRevealed()
    {
        return revealed;
    }
    public void SetGuess(int val)
    {
        guess = val;
        if (revealed)
        {
            return;
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
        if (revealed)
        {
            return;
        }
        guess = 0;
        text.GetComponent<TMP_Text>().text = "";
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
