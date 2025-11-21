using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;
    public float distance = 5f;
    public float height   = 2f;

    [Header("Speed & Clamp")]
    public bool isInverted = false;
    public float rotationSpeed = 180f;
    public float minY = -20f;
    public float maxY =  80f;

    [Header("Gamepad Sensitivity")]
    [Tooltip("ゲームパッド入力時の感度倍率（マウスとの感度差を調整）")]
    public float gamepadSensitivityMultiplier = 5.0f;

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
        _lookInput = value.Get<Vector2>();

        // ゲームパッド入力の場合は感度倍率を適用
        // ゲームパッドは-1.0～1.0に正規化されているため、マウスデルタより値が小さい
        if (Gamepad.current != null &&
            Mathf.Abs(_lookInput.x) <= 1.1f && Mathf.Abs(_lookInput.y) <= 1.1f)
        {
            _lookInput *= gamepadSensitivityMultiplier;
        }
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
        var rot = Quaternion.Euler(_currentY, _currentX, 0f);
        if (isInverted)
            rot = Quaternion.Euler(-_currentY, -_currentX, 0f);
        var    off = rot * new Vector3(0f, height, -distance);
        var    tgtPos = target.position + off;

        // 位置を Lerp で追従
        transform.position = Vector3.Lerp(
            transform.position, tgtPos, followSpeed * Time.deltaTime);

        // 向きを Slerp で追従
        var lookAt = target.position + Vector3.up * height;
        var tgtRot = Quaternion.LookRotation(lookAt - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation, tgtRot, rotateSmooth * Time.deltaTime);
    }
}