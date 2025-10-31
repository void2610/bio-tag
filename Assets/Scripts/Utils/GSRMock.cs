using UnityEngine;
using VContainer.Unity;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// GSRデータのモック生成サービス
/// VContainerで管理され、毎フレームランダムなGSRデータを生成
/// VitalRouterでGsrDataReceivedCommandを発行してGsrGraphにデータを送信
/// </summary>
public class GsrMock : ITickable
{
    private float _current;

    /// <summary>
    /// 毎フレーム呼び出される (VContainer ITickable)
    /// </summary>
    public void Tick()
    {
        // ランダムにGSRデータを生成してCommandで送信
        Router.Default.PublishAsync(new GsrDataReceivedCommand(_current));
        _current += Random.Range(-0.5f, 0.5f);
    }
}
