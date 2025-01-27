using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    public static CameraMove Instance { get; private set; }
    private Vector3 _initPosition;
    private Tween _cameraShakeTween;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        // 初期位置を保存
        _initPosition = this.transform.position;
    }
    
    public void StartShake(float strength)
    {
        var s = Mathf.Min(7.5f, strength);
        _cameraShakeTween?.Kill();
        _cameraShakeTween = this.transform.DOShakePosition(0.1f, s, 10, 0, false).SetLoops(-1);
    }
    
    public void StopShake()
    {
        _cameraShakeTween?.Kill();
        this.transform.position = _initPosition;
    }

    public void ShakeCamera(float duration, float strength)
    {
        var s = Mathf.Min(7.5f, strength);
        this.transform.DOShakePosition(duration, s, 10, 0, false).OnComplete(() =>
        {
            this.transform.position = _initPosition;
        });
    }
}