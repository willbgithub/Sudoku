using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject background, text;
    private int value, guess; // value is 0 if not locked
    private Vector2Int coordinate, box;
    [SerializeField] private Color defaultColor, transparent, selected, defaultText, selectedText, penText;
    private bool revealed = false;

    public void SetColor(int val) // 0 = default, 1 = transparent, 2 = selected
    {
        if (val == 0)
        {
            background.GetComponent<Image>().color = defaultColor;
            text.GetComponent<TMP_Text>().color = defaultText;  
        }
        else if (val == 1)
        {
            background.GetComponent<Image>().color = transparent;
            text.GetComponent<TMP_Text>().color = selectedText;
        }
        else
        {
            background.GetComponent<Image>().color = selected;
            text.GetComponent<TMP_Text>().color = selectedText;
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
    public void Clear()
    {
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
