using System;

[Serializable]
public class DialogueConditionData : BaseData
{
    public string ConditionId => Id;
    public string ConditionType;
    public string Operator;
    public string Value;
    public string NextConditionId;
}