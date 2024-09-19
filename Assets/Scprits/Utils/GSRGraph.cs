using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
    private float threshold = 5f;
    [SerializeField]
    private Vector2 panelStartPos;
    [SerializeField]
    private Vector2 panelEndPos;
    [SerializeField]
    private float ZPos = 1000;
    [SerializeField]
    private Material lineMaterial;
    public bool isExcited { get; private set; } = false;
    public List<Vector3> data = new List<Vector3>();
    private LineRenderer lr;
    private LineRenderer thresholdLine1;
    private LineRenderer thresholdLine2;
    private float max = 10;
    private float min = -10;

    public void SetThreshold(float t)
    {
        threshold = t;
    }

    public void AddData(float d)
    {
        for (int i = 0; i < dataLength - 1; i++)
        {
            data[i] = data[i + 1];
        }
        data[dataLength - 1] = new Vector3(0, d, 0);
        AdjustAndApplyData();
    }

    private void AdjustAndApplyData()
    {
        max = data.Max(v => v.y);
        min = data.Min(v => v.y);
        float range = max - min;
        if (Mathf.Approximately(range, 0f))
        {
            range = 1f;
        }

        var d = data.Select((v, i) =>
        {
            float normalizedY = (v.y - min) / range;
            float xPos = panelStartPos.x + i * (panelEndPos.x - panelStartPos.x) / (dataLength - 1);
            float yPos = normalizedY * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
            return new Vector3(xPos, yPos, ZPos);
        }).ToArray();

        lr.positionCount = dataLength;
        lr.SetPositions(d);
    }

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = lineMaterial;
        thresholdLine1 = this.transform.Find("th1").GetComponent<LineRenderer>();
        thresholdLine2 = this.transform.Find("th2").GetComponent<LineRenderer>();
        thresholdLine1.positionCount = 2;
        thresholdLine2.positionCount = 2;

        data = new List<Vector3>(dataLength);
        for (int i = data.Count; i < dataLength; i++)
        {
            data.Add(Vector3.zero);
        }

        for (int i = 0; i < dataLength; i++)
        {
            AddData(0);
        }
    }

    private void Update()
    {
        float range = max - min;
        if (Mathf.Approximately(range, 0f))
        {
            range = 1f;
        }
        float t1 = (threshold - min) / range;
        float t2 = (-threshold - min) / range;
        t1 = t1 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
        t2 = t2 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;

        thresholdLine1.SetPosition(0, new Vector3(panelStartPos.x, t1, 1000));
        thresholdLine1.SetPosition(1, new Vector3(panelEndPos.x, t1, 1000));
        thresholdLine2.SetPosition(0, new Vector3(panelStartPos.x, t2, 1000));
        thresholdLine2.SetPosition(1, new Vector3(panelEndPos.x, t2, 1000));

        isExcited = data[data.Count - 1].y > threshold || data[data.Count - 1].y < -threshold;
        lr.material.color = isExcited ? Color.red : Color.white;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            threshold--;
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            threshold++;
        }
    }
}
