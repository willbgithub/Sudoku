using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject text;
    private int value; // 0 if not locked
    private Vector2Int coordinate;

    public void SetValue(int val)
    {
        value = val;
        text.GetComponent<TMP_Text>().text = value.ToString();
    }
    public int GetValue()
    {
        return value;
    }
    public void SetCoordinate(Vector2Int val)
    {
        coordinate = val;
    }
    public Vector2Int GetCoordinate()
    {
        return coordinate;
    }
}
