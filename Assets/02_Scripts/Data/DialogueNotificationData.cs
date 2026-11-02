
using System;

[Serializable]
public class DialogueNotificationData : BaseData
{
    public string NotificationId => Id;
    public string Message;
    public string Type;
}