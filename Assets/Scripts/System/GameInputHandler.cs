using System;
using UnityEngine.InputSystem;

/// <summary>
/// ゲーム開始などのグローバル入力を処理するハンドラ
/// Controls C#バインディングを使用
/// </summary>
public class GameInputHandler : IDisposable
{
    private readonly Controls _controls;

    /// <summary>
    /// Ready入力が押されたかどうか
    /// </summary>
    public bool IsReadyPressed { get; private set; }

    /// <summary>
    /// デバッグ用：Gキーが押されたかどうか
    /// </summary>
    public bool IsDebugChangeItPressed => Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;

    public GameInputHandler()
    {
        _controls = new Controls();
        _controls.Player.Enable();
        _controls.Player.Ready.performed += OnReadyPerformed;
    }

    public void Dispose()
    {
        _controls.Player.Ready.performed -= OnReadyPerformed;
        _controls.Player.Disable();
        _controls.Dispose();
    }

    private void OnReadyPerformed(InputAction.CallbackContext context)
    {
        IsReadyPressed = true;
    }

    /// <summary>
    /// Ready入力の状態をリセット
    /// </summary>
    public void ConsumeReadyInput()
    {
        IsReadyPressed = false;
    }
}
