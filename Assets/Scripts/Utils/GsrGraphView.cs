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
    [SerializeField] private int dataLength = 500;
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
        Debug.Assert(_lr != null, "UILineRenderer component is missing.");
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
        if (gsrHistory.Count == 0) return;

        // グラフ描画用にデータを調整
        AdjustAndApplyData(gsrHistory);

        // 閾値線を更新
        UpdateThresholdLines();

        // 閾値の手動調整（デバッグ用）
        if (Input.GetKeyDown(KeyCode.Y))
            _gsrProcessor.AdjustThreshold(-1f);
        else if (Input.GetKeyDown(KeyCode.U))
            _gsrProcessor.AdjustThreshold(1f);
    }

    /// <summary>
    /// グラフデータを正規化して描画
    /// </summary>
    private void AdjustAndApplyData(List<float> gsrHistory)
    {
        _max = gsrHistory.Max();
        _min = gsrHistory.Min();
        _max = Mathf.Max(_max, _gsrProcessor.CurrentThreshold * 1.5f);
        _min = Mathf.Min(_min, -_gsrProcessor.CurrentThreshold * 1.5f);

        var range = _max - _min;
        if (Mathf.Approximately(range, 0f))
            range = 1f;

        var normalizedData = gsrHistory.Select((v, i) =>
        {
            var normalizedY = (v - _min) / range;
            var xPos = i * v1 / (dataLength - 1);
            var yPos = normalizedY * v2;
            return new Vector2(xPos, yPos);
        }).ToArray();

        _lastData = normalizedData[Random.Range(0, normalizedData.Length)];

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
