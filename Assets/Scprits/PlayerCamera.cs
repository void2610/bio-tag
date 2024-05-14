using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target; // プレイヤーをターゲットとして設定
    public float distance; // ターゲットからの距離
    public float height; // カメラの高さ
    public float rotationSpeed; // 回転速度
    private float currentDistance;
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void Start()
    {
        currentDistance = distance;
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        // マウス入力を取得
        currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, -20f, 80f);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // 回転行列を作成
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, height, -currentDistance);
        Vector3 position = target.position + rotation * direction;

        // カメラを設定
        transform.position = position;
        transform.LookAt(target.position + Vector3.up * height);
    }
}
