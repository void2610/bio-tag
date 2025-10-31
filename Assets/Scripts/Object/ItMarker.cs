using UnityEngine;
using DG.Tweening;
using VitalRouter;
using BioTag.GameUI;

/// <summary>
/// "It"プレイヤーの頭上に表示されるマーカー
/// VitalRouterでItChangedCommandを受信して追跡対象を更新
/// </summary>
[Routes]
public partial class ItMarker : MonoBehaviour
{
    private Transform _target;
    private readonly Vector3 _offset = new(0, 2f, 0);
    private const float FLOAT_DISTANCE = 0.1f;
    private float _floatOffsetY;

    /// <summary>
    /// ItChangedCommandハンドラ
    /// 鬼プレイヤーが変更されたらマーカーの追跡対象を更新
    /// </summary>
    [Route]
    private void On(ItChangedCommand cmd)
    {
        this._target = cmd.TargetTransform;
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
        if (this == null || transform == null) return;

        // floatOffsetYをアニメーションさせる
        DOTween.Sequence()
            .Append(DOTween.To(() => _floatOffsetY, x => _floatOffsetY = x, FLOAT_DISTANCE, 1f).SetEase(Ease.InOutSine))
            .Append(DOTween.To(() => _floatOffsetY, x => _floatOffsetY = x, -FLOAT_DISTANCE / 2, 1f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo)
            .Play();
    }

    private void Update()
    {
        if (!_target)
        {
            transform.position = new Vector3(0, -100, 0);
        }
        else
        {
            // ターゲットの位置＋オフセット＋浮動分を設定
            transform.position = _target.position + _offset + new Vector3(0, _floatOffsetY, 0);
        }
    }
}
