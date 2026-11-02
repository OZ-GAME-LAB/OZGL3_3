using TMPro;
using UnityEngine;

public class DialogueUI : NpcUIBase
{
    [SerializeField] private TextMeshProUGUI _speakerText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private UIButton _nextButton;
    [SerializeField] private UIButton _skipButton;

    protected override NpcType ScreenType => NpcType.Story;

    protected override void OnEnable()
    {
        base.OnEnable();
        _nextButton.BindOnClickButtonEvent(Advance);
        _skipButton.BindOnClickButtonEvent(Skip);
    }

    protected override void OnDisable()
    {
        _nextButton.UnBindOnClickButtonEvent(Advance);
        _skipButton.UnBindOnClickButtonEvent(Skip);
        base.OnDisable();
    }

    protected override void Refresh()
    {
        _speakerText.text = Npc.Dialogue.CurrentLine?.SpeakerId;
        _dialogueText.text = Npc.Dialogue.VisibleText;
    }

    public void Advance()
    {
        Npc.Dialogue.Advance();
    }

    public void Skip()
    {
        Npc.Dialogue.Skip();
    }

    protected override void CloseWindow()
    {
        UIManager.Instance.CloseDialoagueUI();
    }
}
