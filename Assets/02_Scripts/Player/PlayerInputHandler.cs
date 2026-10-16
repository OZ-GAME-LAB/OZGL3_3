using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool UpInput { get; private set; }
    public bool DownInput { get; private set; }
    public bool JumpInput { get; private set; }
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
            UpInput = false;
            DownInput = false;
            JumpInput = false;
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

        UpInput = keyboard.upArrowKey.isPressed;
        DownInput = keyboard.downArrowKey.isPressed;
        JumpInput = keyboard.cKey.isPressed;
        CrouchInput = DownInput;
        AttackInput = keyboard.xKey.isPressed;

        bool wasPressCKey = keyboard.cKey.wasPressedThisFrame;
        if (wasPressCKey)
        {
            JumpQueued = true;
        }

        bool wasPressXKey = keyboard.xKey.wasPressedThisFrame;
        if (wasPressXKey)
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
