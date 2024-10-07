using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using ScriptableObjectArchitecture;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input System/Input Reader")]
public class InputReader : ScriptableObjectPlayMode, PlayerInputSystem.IPlayerActions, PlayerInputSystem.IUIActions
{
    private PlayerInputSystem playerInput;

    // Renamed events to avoid conflicts with method names
    public event Action<Vector2> MovePerformed;

    // Maintain state
    public bool IsGameplayActive { get; private set; } = true;

    private void InitializeInput()
    {
        if (playerInput == null)
        {
            playerInput = new PlayerInputSystem();
            playerInput.Player.SetCallbacks(this);
            playerInput.UI.SetCallbacks(this);
        }

        SetGameplayActive(true);
    }

    protected override void InitializeRuntimeData()
    {
        InitializeInput();
    }

    protected override void CleanupRuntimeData()
    {
        CleanupInput();
    }

    private void CleanupInput()
    {
        if (playerInput != null)
        {
            playerInput.Player.Disable();
            playerInput.UI.Disable();
            playerInput.Dispose();
            playerInput = null;
        }
    }

    public void SetGameplayActive(bool isActive)
    {
        IsGameplayActive = isActive;
        if (isActive)
        {
            playerInput.Player.Enable();
            playerInput.UI.Disable();
        }
        else
        {
            playerInput.Player.Disable();
            playerInput.UI.Enable();
        }
    }
    
    // IPlayerActions implementation
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Canceled)
        {
            MovePerformed?.Invoke(context.ReadValue<Vector2>());
        }
    }
}