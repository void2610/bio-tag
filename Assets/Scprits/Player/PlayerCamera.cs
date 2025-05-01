using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public float distance = 5f;
    public float height   = 2f;

    [Header("Speed & Clamp")]
    public float rotationSpeed = 180f;
    public float minY = -20f;
    public float maxY =  80f;

    [Header("Smooth")]
    public float followSpeed   = 10f;
    public float rotateSmooth  = 10f;

    // ──────────────────────────────────────
    private float _currentX;
    private float _currentY;
    private Vector2 _lookInput;   // ← 入力値を保持

    // 入力イベント
    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>(); // ここでは代入だけ
    }

    private void Update()
    {
        // 毎フレーム入力を反映
        _currentX += _lookInput.x * rotationSpeed * Time.deltaTime;
        _currentY -= _lookInput.y * rotationSpeed * Time.deltaTime;
        _currentY = Mathf.Clamp(_currentY, minY, maxY);
    }

    private void LateUpdate()
    {
        if (!target) return;

        // 目標回転と位置
        Quaternion rot = Quaternion.Euler(_currentY, _currentX, 0f);
        Vector3    off = rot * new Vector3(0f, height, -distance);
        Vector3    tgtPos = target.position + off;

        // 位置を Lerp で追従
        transform.position = Vector3.Lerp(
            transform.position, tgtPos, followSpeed * Time.deltaTime);

        // 向きを Slerp で追従
        Vector3 lookAt = target.position + Vector3.up * height;
        Quaternion tgtRot = Quaternion.LookRotation(lookAt - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation, tgtRot, rotateSmooth * Time.deltaTime);
    }
}