using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class GsrGraph : MonoBehaviour
{
    public static GsrGraph Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    [SerializeField] private int dataLength = 500;
    [SerializeField] private float threshold = 5f;
    [SerializeField] private float threshold2 = 1.5f;
    [SerializeField] private float checkLength = 0.1f;
    [SerializeField] private Vector2 panelStartPos;
    [SerializeField] private Vector2 panelEndPos;
    [SerializeField] private Material lineMaterial;
    public bool IsExcited { get; private set; } = false;
    public List<Vector3> data = new ();
    private LineRenderer _lr;
    private LineRenderer _thresholdLine1;
    private LineRenderer _thresholdLine2;
    private float _max = 10;
    private float _min = -10;

    public void SetThreshold(float t) => threshold = t;

    public void AddData(float d)
    {
        for (var i = 0; i < dataLength - 1; i++)
            data[i] = data[i + 1];
        data[dataLength - 1] = new Vector3(0, d, 0);
        AdjustAndApplyData();
    }

    private void AdjustAndApplyData()
    {
        _max = data.Max(v => v.y);
        _min = data.Min(v => v.y);
        _max = Mathf.Max(_max, threshold * 1.5f);
        _min = Mathf.Min(_min, -threshold * 1.5f);

        var range = _max - _min;
        if (Mathf.Approximately(range, 0f))
            range = 1f;

        var d = data.Select((v, i) =>
        {
            var normalizedY = (v.y - _min) / range;
            var xPos = panelStartPos.x + i * (panelEndPos.x - panelStartPos.x) / (dataLength - 1);
            var yPos = normalizedY * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
            return this.transform.position + new Vector3(xPos, yPos, 0);
        }).ToArray();

        _lr.positionCount = dataLength;
        _lr.SetPositions(d);
    }

    private bool CheckExcited(List<float> d)
    {
        // 最新の値をチェック
        if (Mathf.Abs(d[^1]) > threshold)
            return true;

        // 過去の値をチェック（閾値の1.5倍を大きく超えると定義）
        var significantThreshold = threshold * threshold2;
        for (var i = d.Count - 1; i >= 0 && d.Count - 1 - i < checkLength * dataLength; i--)
        {
            if (Mathf.Abs(d[i]) > significantThreshold) return true;
        }

        return false;
    }

    private void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.material = lineMaterial;
        _thresholdLine1 = this.transform.Find("th1").GetComponent<LineRenderer>();
        _thresholdLine2 = this.transform.Find("th2").GetComponent<LineRenderer>();
        _thresholdLine1.positionCount = 2;
        _thresholdLine2.positionCount = 2;

        data = new List<Vector3>(dataLength);
        for (var i = data.Count; i < dataLength; i++)
            data.Add(Vector3.zero);

        for (var i = 0; i < dataLength; i++)
            AddData(0);
    }

    private void Update()
    {
        var range = _max - _min;
        if (Mathf.Approximately(range, 0f)) range = 1f;
        var t1 = (threshold - _min) / range;
        var t2 = (-threshold - _min) / range;
        t1 = t1 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;
        t2 = t2 * (panelEndPos.y - panelStartPos.y) + panelStartPos.y;

        _thresholdLine1.SetPosition(0, new Vector3(panelStartPos.x, t1, 0) + this.transform.position);
        _thresholdLine1.SetPosition(1, new Vector3(panelEndPos.x, t1, 0) + this.transform.position);
        _thresholdLine2.SetPosition(0, new Vector3(panelStartPos.x, t2, 0) + this.transform.position);
        _thresholdLine2.SetPosition(1, new Vector3(panelEndPos.x, t2, 0) + this.transform.position);

        IsExcited = CheckExcited(data.Select(v => v.y).ToList());
        _lr.material.color = IsExcited ? Color.red : Color.white;

        if (Input.GetKeyDown(KeyCode.Y))
            threshold -= 0.1f;
        else if (Input.GetKeyDown(KeyCode.U))
            threshold += 0.1f;
    }
}
