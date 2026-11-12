using UnityEngine;

public abstract class NpcUIBase : UIBase
{
    [SerializeField] private UIButton _closeButton;
    [SerializeField] private UnityEngine.UI.Image _portrait;

    private NpcManager _npc;

    protected NpcManager Npc => _npc;
    protected abstract NpcType ScreenType { get; }

    protected virtual void OnEnable()
    {
        _closeButton.BindOnClickButtonEvent(Close);
    }

    protected virtual void OnDisable()
    {
        _closeButton.UnBindOnClickButtonEvent(Close);
        NpcManager npc = _npc;
        if (_npc == null)
        {
            return;
        }

        _npc = null;
        npc.OnChanged -= HandleChanged;
        if (npc.CurrentType == ScreenType)
        {
            npc.Close();
        }
    }

    public void Init(NpcManager npc)
    {
        if (_npc != null)
        {
            _npc.OnChanged -= HandleChanged;
        }

        _npc = npc;
        _npc.OnChanged += HandleChanged;
        if (_portrait != null)
        {
            _portrait.sprite = npc.Portrait;
            _portrait.enabled = npc.Portrait != null;
        }
        Refresh();
    }

    public void Close()
    {
        _npc.Close();
    }

    private void HandleChanged()
    {
        if (_npc.CurrentType != ScreenType)
        {
            CloseWindow();
            return;
        }

        Refresh();
    }

    protected abstract void Refresh();
    protected abstract void CloseWindow();
}
