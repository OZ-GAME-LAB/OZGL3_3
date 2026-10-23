using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool DashQueued { get; private set; }
    public bool AttackInput { get; private set; }
    public bool BlockInput { get; private set; }
    public bool UpInput { get; private set; }
    public bool DownInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpQueued { get; private set; }

    private float _doubleTapLimitTime = 0.25f;
    private float _lastLeftPressTime = -1f;
    private float _lastRightPressTime = -1f;
    private float _lastMoveDirection = 1f;
    private bool _isRunning;
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
            BlockInput = false;
            DashInput = false;
            DashQueued = false;
            UpInput = false;
            DownInput = false;
            JumpInput = false;
            JumpQueued = false;
            _attackSign = false;
            return;
        }

        bool leftHeld = keyboard.leftArrowKey.isPressed;
        bool rightHeld = keyboard.rightArrowKey.isPressed;

        bool leftPressed = keyboard.leftArrowKey.wasPressedThisFrame;
        bool rightPressed = keyboard.rightArrowKey.wasPressedThisFrame;

        if (leftPressed)
        {
            if (_lastLeftPressTime >= 0f &&
                Time.time - _lastLeftPressTime <= _doubleTapLimitTime)
            {
                _isRunning = true;
            }

            _lastLeftPressTime = Time.time;
        }

        if (rightPressed)
        {
            if (_lastRightPressTime >= 0f &&
                Time.time - _lastRightPressTime <= _doubleTapLimitTime)
            {
                _isRunning = true;
            }

            _lastRightPressTime = Time.time;
        }

        if (leftPressed && !rightPressed)
        {
            _lastMoveDirection = -1f;
        }
        else if (rightPressed && !leftPressed)
        {
            _lastMoveDirection = 1f;
        }

        if (leftHeld && rightHeld)
        {
            MoveInput = _lastMoveDirection;
        }
        else if (leftHeld)
        {
            MoveInput = -1f;
        }
        else if (rightHeld)
        {
            MoveInput = 1f;
        }
        else
        {
            MoveInput = 0f;
            _isRunning = false;
        }

        RunInput = _isRunning && (leftHeld || rightHeld);
        DashInput = keyboard.vKey.isPressed;
        if (keyboard.vKey.wasPressedThisFrame)
        {
            DashQueued = true;
        }

        UpInput = keyboard.upArrowKey.isPressed;
        DownInput = keyboard.downArrowKey.isPressed;
        JumpInput = keyboard.spaceKey.isPressed;
        CrouchInput = DownInput;
        AttackInput = keyboard.xKey.isPressed;
        BlockInput = keyboard.cKey.isPressed;

        bool wasPressCKey = keyboard.spaceKey.wasPressedThisFrame;
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

    public bool ConsumeDashInput()
    {
        bool wasPressed = DashQueued;
        DashQueued = false;
        return wasPressed;
    }

    public void ClearJumpInput()
    {
        JumpQueued = false;
    }

    public void ClearInput()
    {
        MoveInput = 0f;
        RunInput = false;
        CrouchInput = false;
        DashInput = false;
        DashQueued = false;
        AttackInput = false;
        UpInput = false;
        DownInput = false;
        JumpInput = false;
        JumpQueued = false;
        _isRunning = false;
        _attackSign = false;
    }
}
