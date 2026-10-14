using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool JumpQueued { get; private set; }

    private bool _attackSign;

    public void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            MoveInput = 0f;
            RunInput = false;
            CrouchInput = false;
            AttackInput = false;
            JumpQueued = false;
            _attackSign = false;
            return;
        }

        MoveInput =
            (keyboard.rightArrowKey.isPressed ? 1f : 0f) -
            (keyboard.leftArrowKey.isPressed ? 1f : 0f);

        RunInput =
            keyboard.leftShiftKey.isPressed ||
            keyboard.rightShiftKey.isPressed;

        CrouchInput = keyboard.downArrowKey.isPressed;
        AttackInput = keyboard.xKey.isPressed;

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            JumpQueued = true;
        }

        if (keyboard.xKey.wasPressedThisFrame)
        {
            _attackSign = true;
        }
    }

    public bool ComsumeJumpInput()
    {
        bool wasPressed = JumpQueued;
        JumpQueued = false;
        return wasPressed;
    }

    public bool ConsumeAttackInput()
    {
        bool wasPressed = _attackSign;
        _attackSign = false;
        return wasPressed;
    }

    public void ClearJumpInput()
    {
        JumpQueued = false;
    }
}
