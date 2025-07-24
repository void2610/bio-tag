using UnityEngine;
using DG.Tweening;

public class ItMarker : MonoBehaviour
{
    private Transform _target;
    private readonly Vector3 _offset = new Vector3(0, 2f, 0);
    private readonly float _floatDistance = 0.1f;
    private float _floatOffsetY = 0f;

    public void SetTarget(Transform target)
    {
        this._target = target;
    }

    private void Start()
    {
        if (this == null || transform == null) return;

        // floatOffsetYをアニメーションさせる
        DOTween.Sequence()
            .Append(DOTween.To(() => _floatOffsetY, x => _floatOffsetY = x, _floatDistance, 1f).SetEase(Ease.InOutSine))
            .Append(DOTween.To(() => _floatOffsetY, x => _floatOffsetY = x, -_floatDistance / 2, 1f).SetEase(Ease.InOutSine))
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
