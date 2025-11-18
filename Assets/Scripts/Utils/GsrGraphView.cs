using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using BioTag.Biometric;

/// <summary>
/// GSRデータのグラフ表示（UI表示専用）
/// データ処理はGsrProcessorServiceが担当
/// </summary>
[RequireComponent(typeof(UILineRenderer))]
public class GsrGraphView : MonoBehaviour
{
    [SerializeField] private int dataLength = 200;
    [SerializeField] private float v1 = 580f;
    [SerializeField] private float v2 = 200f;
    [SerializeField] private Material lineMaterial;

    private GsrProcessorService _gsrProcessor;
    private UILineRenderer _lr;
    private UILineRenderer _thresholdLine1;
    private UILineRenderer _thresholdLine2;
    private float _max = 10;
    private float _min = -10;
    private Vector3 _lastData = Vector3.zero;

    [Inject]
    public void Construct(GsrProcessorService gsrProcessor)
    {
        _gsrProcessor = gsrProcessor;
    }

    /// <summary>
    /// グラフ線の色を設定
    /// </summary>
    public void SetLineColor(Color c) => _lr.material.color = c;

    /// <summary>
    /// 最後のデータ位置を取得（パーティクル配置用）
    /// </summary>
    public Vector3 GetLastData() => _lastData;

    private void Awake()
    {
        _lr = GetComponent<UILineRenderer>();
        _lr.material = Instantiate(lineMaterial);
        _thresholdLine1 = transform.Find("th1").GetComponent<UILineRenderer>();
        _thresholdLine2 = transform.Find("th2").GetComponent<UILineRenderer>();
        _thresholdLine1.points = new Vector2[2];
        _thresholdLine2.points = new Vector2[2];
    }

    private void Update()
    {
        if (_gsrProcessor == null) return;

        // GsrProcessorServiceからデータ履歴を取得
        var gsrHistory = _gsrProcessor.GetDataHistory();
        if (gsrHistory.Count < 2) return;

        // 履歴から微分値を計算
        var derivativeHistory = new List<float>();
        for (int i = 1; i < gsrHistory.Count; i++)
        {
            derivativeHistory.Add(gsrHistory[i] - gsrHistory[i - 1]);
        }

        // グラフ描画用にデータを調整
        AdjustAndApplyData(derivativeHistory);

        // 閾値線を更新
        UpdateThresholdLines();

        // 閾値の手動調整（デバッグ用）
        if (Input.GetKeyDown(KeyCode.Y))
            _gsrProcessor.AdjustThreshold(-1f);
        else if (Input.GetKeyDown(KeyCode.U))
            _gsrProcessor.AdjustThreshold(1f);
    }

    /// <summary>
    /// グラフデータを正規化して描画（微分値）
    /// </summary>
    private void AdjustAndApplyData(List<float> derivativeHistory)
    {
        // 微分値は既に変化量なので、そのまま使用
        _max = derivativeHistory.Max();
        _min = derivativeHistory.Min();
        _max = Mathf.Max(_max, _gsrProcessor.CurrentThreshold * 1.5f);
        _min = Mathf.Min(_min, -_gsrProcessor.CurrentThreshold * 1.5f);

        var range = _max - _min;
        if (Mathf.Approximately(range, 0f)) range = 1f;

        var normalizedData = derivativeHistory.Select((v, i) =>
        {
            var normalizedY = (v - _min) / range;
            var xPos = i * v1 / (dataLength - 1);
            var yPos = normalizedY * v2;
            return new Vector2(xPos, yPos);
        }).ToArray();

        _lastData = normalizedData[^1];
        _lr.SetPositions(normalizedData);
    }

    /// <summary>
    /// 閾値線を更新
    /// </summary>
    private void UpdateThresholdLines()
    {
        var range = _max - _min;
        if (Mathf.Approximately(range, 0f)) range = 1f;

        var t1 = (_gsrProcessor.CurrentThreshold - _min) / range;
        var t2 = (-_gsrProcessor.CurrentThreshold - _min) / range;
        t1 *= v2;
        t2 *= v2;

        _thresholdLine1.SetPosition(0, new Vector3(0, t1, 0));
        _thresholdLine1.SetPosition(1, new Vector3(v1, t1, 0));
        _thresholdLine2.SetPosition(0, new Vector3(0, t2, 0));
        _thresholdLine2.SetPosition(1, new Vector3(v1, t2, 0));
    }
}
