using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public bool CrouchQueued { get; private set; }
    public bool DashInput { get; private set; }
    public bool DashQueued { get; private set; }
    public bool AttackInput { get; private set; }
    public bool HealSkillQueued { get; private set; }
    public bool GuardedAttackSkillQueued { get; private set; }
    public bool BlockInput { get; private set; }
    public bool UpInput { get; private set; }
    public bool DownInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpQueued { get; private set; }

    public static PlayerInputHandler ActiveInput { get; private set; }
    public static event Action<PlayerInputHandler> OnActiveInputChanged;
    public event Action<Vector2> OnInteractionRequested;

    private float _doubleTapLimitTime = 0.25f;
    private float _lastLeftPressTime = -1f;
    private float _lastRightPressTime = -1f;
    private float _lastMoveDirection = 1f;

    private bool _isRunning;
    private bool _attackSign;
    private readonly Dictionary<Collider2D, MapChange> _mapPortalsByCollider = new();
    private MapChange _interactionPortal;

    private void OnEnable()
    {
        ActiveInput = this;
        OnActiveInputChanged?.Invoke(this);
    }

    private void OnDisable()
    {
        _mapPortalsByCollider.Clear();
        _interactionPortal = null;

        if (ActiveInput != this)
        {
            return;
        }

        ActiveInput = null;
        OnActiveInputChanged?.Invoke(null);
    }

    public void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        NpcManager npc = GameManager.Instance != null ? GameManager.Npc : null;
        bool blocksGameplayInput = npc?.BlocksGameplayInput == true;

        if (keyboard != null && !blocksGameplayInput && keyboard.gKey.wasPressedThisFrame)
        {
            OnInteractionRequested?.Invoke(transform.position);
            if (npc?.BlocksGameplayInput != true)
            {
                _interactionPortal?.Interact();
            }

            blocksGameplayInput = npc?.IsOpen == true;
        }

        if (keyboard == null || blocksGameplayInput)
        {
            MoveInput = 0f;
            RunInput = false;
            CrouchQueued = false;
            AttackInput = false;
            BlockInput = false;
            DashInput = false;
            DashQueued = false;
            UpInput = false;
            DownInput = false;
            JumpInput = false;
            JumpQueued = false;
            HealSkillQueued = false;
            GuardedAttackSkillQueued = false;
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
        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            CrouchQueued = true;
        }
        AttackInput = keyboard.xKey.isPressed;
        BlockInput = keyboard.cKey.isPressed;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            JumpQueued = true;
        }

        bool wasPressXKey = keyboard.xKey.wasPressedThisFrame;
        if (wasPressXKey)
        {
            _attackSign = true;
        }

        if (keyboard.dKey.wasPressedThisFrame)
        {
            HealSkillQueued = true;
        }

        if (keyboard.sKey.wasPressedThisFrame)
        {
            GuardedAttackSkillQueued = true;
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

    public bool ConsumeCrouchInput()
    {
        bool wasPressed = CrouchQueued;
        CrouchQueued = false;
        return wasPressed;
    }

    public bool ConsumeHealSkillInput()
    {
        bool wasPressed = HealSkillQueued;
        HealSkillQueued = false;
        return wasPressed;
    }

    public bool ConsumeGuardedAttackSkillInput()
    {
        bool wasPressed = GuardedAttackSkillQueued;
        GuardedAttackSkillQueued = false;
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
        CrouchQueued = false;
        DashInput = false;
        DashQueued = false;
        AttackInput = false;
        BlockInput = false;
        UpInput = false;
        DownInput = false;
        JumpInput = false;
        JumpQueued = false;
        HealSkillQueued = false;
        _isRunning = false;
        _attackSign = false;
    }

    public void HandlePlayerTriggerEnter(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        MapChange portal = other.GetComponentInParent<MapChange>();
        if (portal == null)
        {
            return;
        }

        _mapPortalsByCollider[other] = portal;
        if (portal.CurrentPortalType == MapPortalType.Interaction)
        {
            _interactionPortal = portal;
        }
    }

    public void HandlePlayerTriggerExit(Collider2D other)
    {
        if (other == null ||
            !_mapPortalsByCollider.TryGetValue(other, out MapChange portal))
        {
            return;
        }

        _mapPortalsByCollider.Remove(other);

        if (_interactionPortal != portal ||
            _mapPortalsByCollider.ContainsValue(portal))
        {
            return;
        }

        _interactionPortal = null;
        foreach (MapChange overlappingPortal in _mapPortalsByCollider.Values)
        {
            if (overlappingPortal != null &&
                overlappingPortal.CurrentPortalType == MapPortalType.Interaction)
            {
                _interactionPortal = overlappingPortal;
            }
        }
    }
}
