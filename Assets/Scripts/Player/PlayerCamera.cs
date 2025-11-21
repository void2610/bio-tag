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

    [Header("Invert Settings")]
    [Tooltip("マウス操作のY軸反転")]
    public bool isMouseInvertedY = false;
    [Tooltip("ゲームパッド操作のY軸反転")]
    public bool isGamepadInvertedY = false;

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
    private bool _isUsingGamepad; // ← 現在のデバイスタイプ

    // 入力イベント
    public void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();

        // デバイス検出：ゲームパッドは-1.0～1.0に正規化されている
        _isUsingGamepad = Gamepad.current != null &&
                          Mathf.Abs(_lookInput.x) <= 1.1f &&
                          Mathf.Abs(_lookInput.y) <= 1.1f;

        if (_isUsingGamepad)
        {
            // ゲームパッド：感度倍率を適用
            _lookInput *= gamepadSensitivityMultiplier;

            // ゲームパッド：Y軸反転処理
            if (isGamepadInvertedY)
            {
                _lookInput.y = -_lookInput.y;
            }
        }
        else
        {
            // マウス：Y軸反転処理
            if (isMouseInvertedY)
            {
                _lookInput.y = -_lookInput.y;
            }
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
        var off = rot * new Vector3(0f, height, -distance);
        var tgtPos = target.position + off;

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