using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcController : MonoBehaviour
{
    private NpcManager _npcManager;
    private Coroutine _pendingDialogue;

    public void Init(NpcManager npcManager)
    {
        _npcManager = npcManager;
        _npcManager.OnScreenChanged += HandleScreenChanged;
        _npcManager.Dialogue.OnRequested += SchedulePendingDialogue;
        UIManager.OnInitialized += SchedulePendingDialogue;
        HandleScreenChanged();
    }

    private void HandleScreenChanged()
    {
        // 비활성 상태에서도 요청 이벤트를 받아야 하므로 구독은 Dispose에서 해제한다.
        enabled = _npcManager.IsOpen;
        SchedulePendingDialogue();
    }

    private void SchedulePendingDialogue()
    {
        if (_pendingDialogue != null || !_npcManager.Dialogue.HasPendingGroups
            || _npcManager.IsOpen || _npcManager.IsChoosingEnding)
        {
            return;
        }

        _pendingDialogue = StartCoroutine(OpenPendingDialogue());
    }

    private IEnumerator OpenPendingDialogue()
    {
        yield return null;
        _pendingDialogue = null;
        if (_npcManager.BlocksGameplayInput || UIManager.Instance == null)
        {
            yield break;
        }

        string groupId = _npcManager.Dialogue.TakePendingGroup();
        if (!string.IsNullOrEmpty(groupId))
        {
            UIManager.Instance.OpenDialogueGroupUI(_npcManager, groupId);
        }
    }

    private void Update()
    {
        if (_npcManager.CurrentType == NpcType.Story)
        {
            _npcManager.Dialogue.Tick(Time.unscaledDeltaTime);
        }
        if (_npcManager.OpenedFrame == Time.frameCount)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_npcManager.CurrentType == NpcType.Story)
            {
                _npcManager.Dialogue.Skip();
            }
            else
            {
                _npcManager.Close();
            }
            return;
        }
        if (_npcManager.CurrentType == NpcType.Story && keyboard.anyKey.wasPressedThisFrame)
        {
            _npcManager.Dialogue.Advance();
        }
    }

    public void Dispose()
    {
        if (_npcManager == null)
        {
            return;
        }
        if (_pendingDialogue != null)
        {
            StopCoroutine(_pendingDialogue);
            _pendingDialogue = null;
        }
        _npcManager.OnScreenChanged -= HandleScreenChanged;
        _npcManager.Dialogue.OnRequested -= SchedulePendingDialogue;
        UIManager.OnInitialized -= SchedulePendingDialogue;
        _npcManager = null;
        enabled = false;
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
