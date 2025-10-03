using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

[RequireComponent(typeof(UILineRenderer))]
public class GsrGraph : MonoBehaviour
{
    [SerializeField] private int dataLength = 500;
    [SerializeField] private float threshold = 5f;
    [SerializeField] private float thresholdMagni = 1.5f;
    [SerializeField] private float checkLength = 0.1f;
    [SerializeField] private float v1 = 580f;
    [SerializeField] private float v2 = 200f;
    [SerializeField] private Material lineMaterial;
    public bool IsExcited { get; private set; } = false;
    public List<Vector2> data = new ();

    // 現在のGSR生値を取得
    public float CurrentGsrRaw { get; private set; } = 0f;

    private UILineRenderer _lr;
    private UILineRenderer _thresholdLine1;
    private UILineRenderer _thresholdLine2;
    private float _max = 10;
    private float _min = -10;
    private Vector3 _lastData = Vector3.zero;
    private float _test = 0f;

    public void SetLineColor(Color c) => _lr.material.color = c;
    public Vector3 GetLastData() => _lastData;
    public void AddData(float d)
    {
        d = Mathf.Clamp(d, 0f, 1024f);
        CurrentGsrRaw = d; // 生値を記録
        // Debug.Log(d);

        for (var i = 0; i < dataLength - 1; i++)
            data[i] = data[i + 1];
        data[dataLength - 1] = new Vector3(0, d, 0);
        AdjustAndApplyData();
    }

    /// <summary>
    /// GSRデータの平均値を計算
    /// </summary>
    public float GetMeanGsr()
    {
        if (data.Count == 0) return 0f;
        return data.Average(v => v.y);
    }

    /// <summary>
    /// GSRデータの標準偏差を計算
    /// </summary>
    public float GetStandardDeviationGsr()
    {
        if (data.Count == 0) return 0f;
        var mean = GetMeanGsr();
        var sumOfSquares = data.Sum(v => (v.y - mean) * (v.y - mean));
        return Mathf.Sqrt(sumOfSquares / data.Count);
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
            var xPos = i * v1 / (dataLength - 1);
            var yPos = normalizedY * v2;
            return new Vector2(xPos, yPos);
        }).ToArray();
        _lastData = d[Random.Range(0, d.Length)];

        _lr.SetPositions(d);
    }

    private bool CheckExcited(List<float> d)
    {
        // 最新の値をチェック
        if (Mathf.Abs(d[^1]) > threshold)
            return true;

        // 過去の値をチェック（閾値の1.5倍を大きく超えると定義）
        var significantThreshold = threshold * thresholdMagni;
        for (var i = d.Count - 1; i >= 0 && d.Count - 1 - i < checkLength * dataLength; i--)
        {
            if (Mathf.Abs(d[i]) > significantThreshold) return true;
        }

        return false;
    }

    private void Awake()
    {
        _lr = this.GetComponent<UILineRenderer>();
        Debug.Assert(_lr != null, "UILineRenderer component is missing.");
        _lr.material = lineMaterial;
        _thresholdLine1 = this.transform.Find("th1").GetComponent<UILineRenderer>();
        _thresholdLine2 = this.transform.Find("th2").GetComponent<UILineRenderer>();
        _thresholdLine1.points = new Vector2[2];
        _thresholdLine2.points = new Vector2[2];
    }

    private void Start()
    {
        data = new List<Vector2>(dataLength);
        for (var i = data.Count; i < dataLength; i++)
            data.Add(Vector2.zero);

        for (var i = 0; i < dataLength; i++)
            AddData(0);
    }

    private void Update()
    {
        var range = _max - _min;
        if (Mathf.Approximately(range, 0f)) range = 1f;
        var t1 = (threshold - _min) / range;
        var t2 = (-threshold - _min) / range;
        t1 *= v2;
        t2 *= v2;

        _thresholdLine1.SetPosition(0, new Vector3(0, t1, 0));
        _thresholdLine1.SetPosition(1, new Vector3(v1, t1, 0));
        _thresholdLine2.SetPosition(0, new Vector3(0, t2, 0));
        _thresholdLine2.SetPosition(1, new Vector3(v1, t2, 0));

        IsExcited = CheckExcited(data.Select(v => v.y).ToList());
        _lr.material.color = IsExcited ? Color.red : Color.white;

        if (Input.GetKeyDown(KeyCode.Y))
            threshold -= 1f;
        else if (Input.GetKeyDown(KeyCode.U))
            threshold += 1f;

        // if (TcpServer.Instance && TcpServer.Instance.IsConnected.CurrentValue)
        // {
        //     AddData(TcpServer.Instance.LastValue.CurrentValue);
        // }
        
        // test
        _test += 1 * Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
        AddData(_test);
    }
}
