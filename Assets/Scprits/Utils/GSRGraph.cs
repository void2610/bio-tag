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

    public List<Vector3> data = new List<Vector3>();
    private LineRenderer lr => GetComponent<LineRenderer>();

    public void AddData(float d)
    {
        for (int i = dataLength - 1; i > 1; i--)
        {
            data[i - 1] = data[i];
        }
        var p = new Vector3(0, d, 0);

        data[dataLength - 1] = p;
        AdjustAndApplyData();
    }

    private void AdjustAndApplyData()
    {
        float max = data[0].y;
        float min = data[0].y;
        List<Vector3> d = new List<Vector3>(data);
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

        float range = max - min;
        if (Mathf.Approximately(range, 0f))
        {
            range = 1f; // ゼロ除算を防ぐ
        }

        for (int i = 0; i < data.Count; i++)
        {
            float normalizedY = (data[i].y - min) / range;
            float xPos = panelStartPos.x + i * (panelEndPos.x - panelStartPos.x) / dataLength;
            float yPos = normalizedY * (panelEndPos.y - panelStartPos.y) + panelStartPos.y; m
            d[i] = new Vector3(xPos, data[i].y, 0);
        }
        lr.positionCount = dataLength;
        lr.SetPositions(d.ToArray());
    }

    private void Start()
    {
        data = new List<Vector3>(dataLength);
        for (int i = data.Count; i < dataLength; i++)
        {
            data.Add(Vector3.zero);
        }

        // すべて0で初期化
        for (int i = 0; i < dataLength; i++)
        {
            AddData(i);
        }
    }
}
