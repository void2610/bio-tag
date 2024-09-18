using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GSRGraph : MonoBehaviour
{
    public static GSRGraph instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [SerializeField]
    private static int dataLength = 100;
    [SerializeField]
    private Vector2 panelStartPos;
    [SerializeField]
    private Vector2 panelEndPos;

    public List<Vector2> data = new List<Vector2>();
    private UILineRenderer lr => GetComponent<UILineRenderer>();

    public void AddData(float d)
    {
        for (int i = dataLength - 1; i > 1; i--)
        {
            data[i] = data[i - 1];
        }
        var p = new Vector2(0, d);

        data[dataLength - 1] = p;
        AdjustAndApplyData();
    }

    private void AdjustAndApplyData()
    {
        float max = data[0].y;
        float min = data[0].y;
        List<Vector2> d = new List<Vector2>(data);
        for (int i = 1; i < data.Count; i++)
        {
            if (d[i].y > max)
            {
                max = d[i].y;
            }
            if (d[i].y < min)
            {
                min = d[i].y;
            }
        }

        for (int i = 0; i < data.Count; i++)
        {
            d[i] = new Vector2(panelStartPos.x + i * (panelEndPos.x - panelStartPos.x) / dataLength, (data[i].y - min) / (max - min) * (panelEndPos.y - panelStartPos.y) + panelStartPos.y);
        }
        lr.Points = d.ToArray();
    }

    private void Start()
    {
        data = new List<Vector2>(dataLength);
        for (int i = data.Count; i < dataLength; i++)
        {
            data.Add(Vector2.zero);
        }

        // すべて0で初期化
        for (int i = 0; i < dataLength; i++)
        {
            AddData(i);
        }
    }
}
