using System;
using UnityEngine;

public enum NpcType
{
    None,
    Shop,
    Artifact,
    Story
}

public class NpcManager : IDisposable
{
    private static readonly Color _overloadedColor = new(0.72f, 0.3f, 0.9f, 1f);
    private int _closedFrame = -1;
    private readonly UserDataManager _userData;
    private bool _isChoosingEnding;

    public DataTableManager Tables { get; }

    public ShopService Shop { get; }
    public ArtifactService Artifacts { get; }
    public DialogueService Dialogue { get; }
    public NpcType CurrentType { get; private set; }
    public string CurrentNpcId { get; private set; }
    public Sprite Portrait { get; private set; }
    public bool IsOpen => CurrentType != NpcType.None;
    public bool IsChoosingEnding => _isChoosingEnding;
    public bool BlocksGameplayInput => IsOpen || _isChoosingEnding || _closedFrame == Time.frameCount;
    public int OpenedFrame { get; private set; }
    public Color EquipmentTint => Artifacts.IsOverloaded ? _overloadedColor : Color.white;

    public event Action OnChanged;
    public event Action OnScreenChanged;

    public NpcManager(DataTableManager tables, UserDataManager userData)
    {
        Tables = tables;
        _userData = userData;
        Shop = new ShopService(tables, userData);
        Artifacts = new ArtifactService(tables, userData);
        Dialogue = new DialogueService(tables, userData);
        userData.OnChanged += NotifyChanged;
        Shop.OnChanged += NotifyChanged;
        Artifacts.OnChanged += NotifyChanged;
        Dialogue.OnChanged += NotifyChanged;
        Dialogue.OnFinished += Close;
    }

    public bool TryOpen(NpcType type, string npcId, Sprite portrait = null)
    {
        if (IsOpen || _isChoosingEnding || type == NpcType.None)
        {
            return false;
        }

        if (type == NpcType.Story && !Dialogue.TryStart(npcId))
        {
            return false;
        }

        Open(type, npcId, portrait);
        return true;
    }

    public bool TryOpenGroup(string dialogueGroupId, Sprite portrait = null)
    {
        if (IsOpen || _isChoosingEnding || !Dialogue.TryStartGroup(dialogueGroupId))
        {
            return false;
        }

        Open(NpcType.Story, Dialogue.CurrentGroup.NpcId, portrait);
        return true;
    }

    public void SetEndingChoiceOpen(bool isOpen)
    {
        _isChoosingEnding = isOpen;
        NotifyChanged();
        OnScreenChanged?.Invoke();
    }

    private void Open(NpcType type, string npcId, Sprite portrait)
    {
        CurrentType = type;
        CurrentNpcId = npcId;
        Portrait = portrait;
        OpenedFrame = Time.frameCount;
        Artifacts.ResetAttempts();
        NotifyChanged();
        OnScreenChanged?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        CurrentType = NpcType.None;
        _closedFrame = Time.frameCount;
        CurrentNpcId = null;
        Portrait = null;
        Artifacts.ResetAttempts();
        Dialogue.Cancel();
        NotifyChanged();
        OnScreenChanged?.Invoke();
    }

    public void Dispose()
    {
        Close();
        _userData.OnChanged -= NotifyChanged;
        Shop.OnChanged -= NotifyChanged;
        Artifacts.OnChanged -= NotifyChanged;
        Dialogue.OnChanged -= NotifyChanged;
        Dialogue.OnFinished -= Close;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
