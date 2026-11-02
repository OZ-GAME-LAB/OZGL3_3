using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataTableManager
{
    public IReadOnlyDictionary<string, ShopData> Shop { get; private set; }
    public IReadOnlyDictionary<string, ArtifactData> Artifact { get; private set; }
    public IReadOnlyDictionary<string, DialogueGroupData> DialogueGroup { get; private set; }
    public IReadOnlyDictionary<string, DialogueConditionData> DialogueCondition { get; private set; }
    public IReadOnlyDictionary<string, DialogueData> Dialogue { get; private set; }
    public IReadOnlyDictionary<string, DialogueNotificationData> DialogueNotification { get; private set; }
    public IReadOnlyDictionary<string, DialogueTriggerData> DialogueTrigger { get; private set; }

    public void LoadAllData()
    {
        Shop = LoadData<ShopData>(nameof(ShopData));
        Artifact = LoadData<ArtifactData>(nameof(ArtifactData));
        Dialogue = LoadData<DialogueData>(nameof(DialogueData));
        DialogueGroup = LoadData<DialogueGroupData>(nameof(DialogueGroupData));
        DialogueCondition = LoadData<DialogueConditionData>(nameof(DialogueConditionData));
        DialogueNotification = LoadData<DialogueNotificationData>(nameof(DialogueNotificationData));
        DialogueTrigger = LoadData<DialogueTriggerData>(nameof(DialogueTriggerData));
    }

    #region Getters


    #endregion

    [Serializable]
    private class SerializationWrapper<T>
    {
        [SerializeField] private List<T> _items;
        public List<T> Items => _items;
    }

    private Dictionary<string, T> LoadData<T>(string tableNmae) where T : BaseData
    {
        string resourcePath = $"JsonOutput/{tableNmae}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
        if (null == textAsset)
        {
            Logger.LogError($"리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            string jsonString = textAsset.text;

            string wrappedJson = "{\"_items\":" + jsonString + "}";

            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper == null || wrapper.Items == null)
            {
                Logger.LogError($"[{typeof(T).Name}] JSON 파싱 결과가 비어 있습니다.");
            }

            if (null != wrapper && null != wrapper.Items)
            {
                Logger.Log($"{typeof(T).Name} 데이터를 {wrapper.Items.Count}개 로드했습니다.");
                return wrapper.Items.ToDictionary(value => value.Id);
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }
}
