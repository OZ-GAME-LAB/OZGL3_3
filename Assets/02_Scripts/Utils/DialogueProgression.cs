using System.Linq;

public static class DialogueProgression
{
    public static bool Apply(DialogueTriggerData trigger, UserDialogueData data)
    {
        bool changed = false;
        if (!string.IsNullOrEmpty(trigger.FlagId) && !data.Flags.Contains(trigger.FlagId))
        {
            data.SetFlag(trigger.FlagId);
            changed = true;
        }

        // -1은 유지 값이며, 이전 지역에 다시 진입해도 진행 단계를 낮추지 않는다.
        if (trigger.StoryStage > data.StoryStage)
        {
            data.StoryStage = trigger.StoryStage;
            changed = true;
        }

        if (!string.IsNullOrEmpty(trigger.EventId) && !data.Events.Contains(trigger.EventId))
        {
            data.SetEvent(trigger.EventId);
            changed = true;
        }

        return changed;
    }
}
