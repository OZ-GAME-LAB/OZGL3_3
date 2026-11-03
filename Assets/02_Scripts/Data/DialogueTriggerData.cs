using System;

[Serializable]
public class DialogueTriggerData : BaseData
{
    public string TriggerId => Id;
    public string ExecutionExample;
    public string Description;

    public string FlagId;
    public int StoryStage = -1;
    public string EventId;
    public string NextDialogueGroupId;
}
