
using System;

[Serializable]
public class DialogueData : BaseData
{
    public string DialogueId => Id;
    public string DialogueGroupId;
    public string SpeakerId;
    public string DialogueText;
    public string NextDialogueId;
    public string RewardItemId;
    public int RewardAmount = 1;
    public string NotificationId;
    public string TriggerId;
}