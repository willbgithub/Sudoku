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
    [SerializeField] private Color defaultColor, transparent, selected, defaultText, selectedText, penText;
    private bool revealed = false;
    private Game game;


    public void OnPointerEnter(BaseEventData data)
    {
        print("OnPointerEnter " + coordinate);
        game.OnCellPointerEnter(this);
    }
    public void OnPointerExit(BaseEventData data)
    {
        print("OnPointerExit " + coordinate);
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
    /// Sets the cell's color. 0 = default, 1 = transparent, 2 = selected
    /// </summary>
    public void SetColor(int val) // 0 = default, 1 = transparent, 2 = selected
    {
        if (val == 0)
        {
            background.GetComponent<Image>().color = defaultColor;
            text.GetComponent<TMP_Text>().color = defaultText;
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Normal;
        }
        else if (val == 1)
        {
            background.GetComponent<Image>().color = transparent;
            text.GetComponent<TMP_Text>().color = selectedText;
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        }
        else if (val == 2)
        {
            background.GetComponent<Image>().color = selected;
            text.GetComponent<TMP_Text>().color = selectedText;
            text.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
        }
        else if (val == 4)
        {
            background.GetComponent<Image>().color = Color.red;
            text.GetComponent<TMP_Text>().color = defaultText;
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
            text.GetComponent<TMP_Text>().text = value.ToString();
        }
        else
        {
            text.GetComponent<TMP_Text>().text = "";
        }
    }
    public bool GetRevealed()
    {
        return revealed;
    }
    public void SetGuess(int val)
    {
        guess = val;
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
        guess = 0;
        if (!revealed)
        {
            text.GetComponent<TMP_Text>().text = "";
        }
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
