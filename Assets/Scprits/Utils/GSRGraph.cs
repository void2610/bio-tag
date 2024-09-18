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
    private int dataLength = 500;
    [SerializeField]
    private Vector2 panelStartPos;
    [SerializeField]
    private Vector2 panelEndPos;
    [SerializeField]
    private float ZPos = 1000;

    public List<Vector3> data = new List<Vector3>();
    private LineRenderer lr;
    private LineRenderer thresholdLine1;
    private LineRenderer thresholdLine2;
    private float threshold = 5f;
    private float max = -99999;
    private float min = 99999;

    public void SetThreshold(float t)
    {
        threshold = t;
    }
    public void AddData(float d)
    {
        // データを左にシフト
        for (int i = 0; i < dataLength - 1; i++)
        {
            data[i] = data[i + 1];
        }

        // 新しいデータを末尾に追加
        data[dataLength - 1] = new Vector3(0, d, 0);

        // データを調整して適用
        AdjustAndApplyData();
    }

    private void AdjustAndApplyData()
    {
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
            float yPos = normalizedY * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
            d[i] = new Vector3(xPos, yPos, 1000);
        }
        lr.positionCount = dataLength;
        lr.SetPositions(d.ToArray());
    }

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        thresholdLine1 = this.transform.Find("th1").GetComponent<LineRenderer>();
        thresholdLine2 = this.transform.Find("th2").GetComponent<LineRenderer>();
        thresholdLine1.positionCount = 2;
        thresholdLine2.positionCount = 2;

        data = new List<Vector3>(dataLength);
        for (int i = data.Count; i < dataLength; i++)
        {
            data.Add(Vector3.zero);
        }

        // すべて0で初期化
        for (int i = 0; i < dataLength; i++)
        {
            AddData(0);
        }
    }

    private void Update()
    {
        float t1 = (threshold - min) / (max - min);
        float t2 = (-threshold - min) / (max - min);
        t1 = t1 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
        t2 = t2 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;

        thresholdLine1.SetPosition(0, new Vector3(panelStartPos.x, t1, 1000));
        thresholdLine1.SetPosition(1, new Vector3(panelEndPos.x, t1, 1000));
        thresholdLine2.SetPosition(0, new Vector3(panelStartPos.x, t2, 1000));
        thresholdLine2.SetPosition(1, new Vector3(panelEndPos.x, t2, 1000));
    }
}
