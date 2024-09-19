using UnityEngine;
using DG.Tweening;

public class ItMarker : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 2f, 0);
    private float floatDistance = 0.1f;
    private float floatOffsetY = 0f;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Start()
    {
        if (this == null || transform == null) return;

        // floatOffsetYをアニメーションさせる
        DOTween.Sequence()
            .Append(DOTween.To(() => floatOffsetY, x => floatOffsetY = x, floatDistance, 1f).SetEase(Ease.InOutSine))
            .Append(DOTween.To(() => floatOffsetY, x => floatOffsetY = x, -floatDistance / 2, 1f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo)
            .Play();
    }

    private void Update()
    {
        if (target == null)
        {
            transform.position = new Vector3(0, -100, 0);
        }
        else
        {
            // ターゲットの位置＋オフセット＋浮動分を設定
            transform.position = target.position + offset + new Vector3(0, floatOffsetY, 0);
        }
    }
}
