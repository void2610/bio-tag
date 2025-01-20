using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target; // プレイヤーをターゲットとして設定
    public float distance; // ターゲットからの距離
    public float height; // カメラの高さ
    public float rotationSpeed; // 回転速度
    private float _currentDistance;
    private float _currentX = 0.0f;
    private float _currentY = 0.0f;

    private void Start()
    {
        _currentDistance = distance;
    }

    private void Update()
    {
        if (!target)
        {
            return;
        }

        // マウス入力を取得
        _currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        _currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        _currentY = Mathf.Clamp(_currentY, -20f, 80f);
    }

    private void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        // 回転行列を作成
        var rotation = Quaternion.Euler(_currentY, _currentX, 0);
        var direction = new Vector3(0, height, -_currentDistance);
        var position = target.position + rotation * direction;

        // カメラを設定
        transform.position = position;
        transform.LookAt(target.position + Vector3.up * height);
    }
}
