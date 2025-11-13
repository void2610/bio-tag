using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// GSRデータのグラフ表示と興奮状態判定
/// VitalRouterでGsrDataReceivedCommandを受信してデータ追加
/// </summary>
[RequireComponent(typeof(UILineRenderer))]
[Routes]
public partial class GsrGraph : MonoBehaviour
{
    [SerializeField] private int filterWindowSize = 10;
    [SerializeField] private int dataLength = 500;
    [SerializeField] private float threshold = 5f;
    [SerializeField] private float thresholdMagni = 1.5f;
    [SerializeField] private float checkLength = 0.1f;
    [SerializeField] private float v1 = 580f;
    [SerializeField] private float v2 = 200f;
    [SerializeField] private Material lineMaterial;
    public bool IsExcited { get; private set; }
    public List<Vector2> data = new ();

    // 現在のGSR生値を取得
    public float CurrentGsrRaw { get; private set; }
    // 現在のGSRフィルタ済み値（移動平均）
    public float CurrentGsrFiltered { get; private set; }
    // 現在のGSR微分値 dG(t)/dt
    public float CurrentGsrDerivative { get; private set; }
    // 現在の閾値θ
    public float CurrentThreshold => threshold;
    
    private bool _previousIsExcited;
    private float _previousGsrRaw;
    private UILineRenderer _lr;
    private UILineRenderer _thresholdLine1;
    private UILineRenderer _thresholdLine2;
    private float _max = 10;
    private float _min = -10;
    private Vector3 _lastData = Vector3.zero;

    public void SetLineColor(Color c) => _lr.material.color = c;
    public Vector3 GetLastData() => _lastData;

    /// <summary>
    /// GSRデータ受信コマンドハンドラ
    /// </summary>
    [Route]
    private void On(GsrDataReceivedCommand cmd)
    {
        AddData(cmd.RawValue);
    }

    /// <summary>
    /// GSRデータをグラフに追加
    /// </summary>
    private void AddData(float d)
    {
        d = Mathf.Clamp(d, 0f, 1024f);

        // 微分値を計算（前回値との差分）
        CurrentGsrDerivative = d - _previousGsrRaw;
        _previousGsrRaw = d;

        CurrentGsrRaw = d; // 生値を記録
        // Debug.Log(d);

        for (var i = 0; i < dataLength - 1; i++)
            data[i] = data[i + 1];
        data[dataLength - 1] = new Vector3(0, d, 0);

        // フィルタ済み値を計算（移動平均）
        CurrentGsrFiltered = CalculateFilteredValue();

        AdjustAndApplyData();
    }

    /// <summary>
    /// フィルタ済みGSR値を計算（移動平均）
    /// </summary>
    private float CalculateFilteredValue()
    {
        if (data.Count == 0) return 0f;

        var windowSize = Mathf.Min(filterWindowSize, data.Count);
        var sum = 0f;
        for (var i = data.Count - windowSize; i < data.Count; i++)
        {
            sum += data[i].y;
        }
        return sum / windowSize;
    }

    /// <summary>
    /// GSRデータの平均値を計算
    /// </summary>
    private float GetMeanGsr()
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
        _lr.material = Instantiate(lineMaterial);
        _thresholdLine1 = this.transform.Find("th1").GetComponent<UILineRenderer>();
        _thresholdLine2 = this.transform.Find("th2").GetComponent<UILineRenderer>();
        _thresholdLine1.points = new Vector2[2];
        _thresholdLine2.points = new Vector2[2];
    }

    private void OnEnable()
    {
        // VitalRouterのデフォルトルーターに登録
        this.MapTo(Router.Default);
    }

    private void OnDisable()
    {
        // VitalRouterから登録解除
        this.UnmapRoutes();
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

        // 状態変化を検知してCommandを発行
        if (IsExcited != _previousIsExcited)
        {
            var newState = IsExcited ? BiometricState.Excited : BiometricState.Calm;
            var previousState = _previousIsExcited ? BiometricState.Excited : BiometricState.Calm;
            var intensity = CurrentGsrRaw;

            Router.Default.PublishAsync(new BiometricStateChangedCommand(newState, previousState, intensity));

            _previousIsExcited = IsExcited;
        }

        // 閾値の手動調整（デバッグ用）
        if (Input.GetKeyDown(KeyCode.Y))
            threshold -= 1f;
        else if (Input.GetKeyDown(KeyCode.U))
            threshold += 1f;
    }
}
