
using System;

[Serializable]
public class DialogueGroupData : BaseData
{
    public string DialogueGroupId => Id;
    public string NpcId;
    public string StartDialogueId;
    public string ConditionId;
    public bool IsRepeatable;

    public int Priority;
}
