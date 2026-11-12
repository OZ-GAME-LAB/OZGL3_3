using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ArtifactUI : NpcUIBase
{
    [SerializeField] private UIButton[] _slotButtons;
    [SerializeField] private UIButton _equipButton;
    [SerializeField] private UIButton _unequipButton;
    [SerializeField] private TextMeshProUGUI _equipmentText;
    [SerializeField] private TextMeshProUGUI _gaugeText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    private ArtifactSlotState[] _slots = Array.Empty<ArtifactSlotState>();
    private readonly Dictionary<string, Sprite> _artifactIcons = new();
    private readonly List<UnityEngine.UI.Image> _slotIcons = new();
    private readonly List<UnityEngine.UI.Image> _equippedIcons = new();
    private readonly List<UnityEngine.UI.Image> _chaosGaugeIcons = new();
    private bool _isVisualsBuilt;

    protected override NpcType ScreenType => NpcType.Artifact;

    protected override void OnEnable()
    {
        base.OnEnable();
        BuildVisuals();
        UnityEngine.UI.Button equipButton = _equipButton.GetComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.Button unequipButton = _unequipButton.GetComponent<UnityEngine.UI.Button>();
        unequipButton.targetGraphic.color = equipButton.targetGraphic.color;
        _equipButton.BindOnClickButtonEvent(Equip);
        _unequipButton.BindOnClickButtonEvent(Unequip);
        for (int index = 0; index < _slotButtons.Length; index++)
        {
            int slot = index;
            _slotButtons[index].BindOnClickButtonEvent(() => SelectArtifact(slot), true);
        }
    }

    protected override void OnDisable()
    {
        _equipButton.UnBindOnClickButtonEvent(Equip);
        _unequipButton.UnBindOnClickButtonEvent(Unequip);
        foreach (UIButton button in _slotButtons)
        {
            button.UnBindAllOnClickButtonEvent();
        }

        base.OnDisable();
    }

    protected override void Refresh()
    {
        _slots = Npc.Artifacts.GetSlots().ToArray();
        for (int index = 0; index < _slotButtons.Length; index++)
        {
            bool exists = index < _slots.Length;
            bool isOwned = exists && _slots[index].IsOwned;
            bool isEquipped = isOwned && _slots[index].IsEquipped;
            _slotButtons[index].SetInteractable(isOwned);
            _slotButtons[index].ChangeButtonText(string.Empty);
            _slotButtons[index].GetComponent<UnityEngine.UI.Image>().color = isOwned
                ? Color.clear
                : Color.white;
            _slotIcons[index].enabled = isOwned;
            _slotIcons[index].color = isEquipped
                ? new Color(0.65f, 0.65f, 0.65f, 1f)
                : Color.white;
            if (isOwned)
            {
                _slotIcons[index].sprite = GetArtifactIcon(_slots[index].ArtifactId);
            }
        }

        RefreshEquippedIcons();
        RefreshChaosGauge();

        ArtifactData selected = Npc.Artifacts.SelectedArtifact;
        bool equipped = selected != null && Npc.Artifacts.EquippedArtifactIds.Contains(selected.Id);
        bool hasCost = selected != null && selected.ChaosCost >= 0;
        _equipButton.SetInteractable(hasCost && !equipped
            && Npc.Artifacts.CurrentChaosGauge < Npc.Artifacts.MaxChaosGauge);
        _unequipButton.SetInteractable(equipped);
        _descriptionText.text = selected == null ? "소유한 아티펙트를 선택하세요."
            : $"{selected.Name}\n\n{selected.Desc}\n\n혼돈: {(hasCost ? selected.ChaosCost.ToString() : "미설정")}";
    }

    private void BuildVisuals()
    {
        if (_isVisualsBuilt)
        {
            return;
        }

        foreach (UIButton button in _slotButtons)
        {
            UnityEngine.UI.Image slotBackground = button.GetComponent<UnityEngine.UI.Image>();
            slotBackground.sprite = Resources.LoadAll<Sprite>("Sprites/Icon/elpida_ui_equipped_relic_symbol (1)")[0];
            slotBackground.preserveAspect = true;
            _slotIcons.Add(CreateIcon(button.transform, "ArtifactIcon", 120));
        }

        for (int index = 0; index < _slotButtons.Length; index++)
        {
            UnityEngine.UI.Image icon = CreateIcon(_equipmentText.transform, "EquippedArtifactIcon", 112);
            RectTransform rectTransform = icon.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(index * 124, 0f);
            _equippedIcons.Add(icon);
        }

        _equipmentText.text = string.Empty;
        _gaugeText.text = string.Empty;
        _isVisualsBuilt = true;
    }

    private void RefreshEquippedIcons()
    {
        IReadOnlyList<string> equippedArtifactIds = Npc.Artifacts.EquippedArtifactIds;
        for (int index = 0; index < _equippedIcons.Count; index++)
        {
            bool isEquipped = index < equippedArtifactIds.Count;
            _equippedIcons[index].enabled = isEquipped;
            if (isEquipped)
            {
                _equippedIcons[index].sprite = GetArtifactIcon(equippedArtifactIds[index]);
            }
        }
    }

    private void RefreshChaosGauge()
    {
        int currentGauge = Npc.Artifacts.CurrentChaosGauge;
        while (_chaosGaugeIcons.Count < currentGauge)
        {
            UnityEngine.UI.Image icon = CreateIcon(_gaugeText.transform, "ChaosGaugeIcon", 28);
            icon.sprite = Resources.LoadAll<Sprite>("Sprites/Icon/elpida_chaos_gauge_icon")[0];
            RectTransform rectTransform = icon.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(_chaosGaugeIcons.Count * 34, 0f);
            _chaosGaugeIcons.Add(icon);
        }

        Color gaugeColor = Npc.Artifacts.IsOverloaded ? Npc.EquipmentTint : Color.white;
        for (int index = 0; index < _chaosGaugeIcons.Count; index++)
        {
            _chaosGaugeIcons[index].enabled = index < currentGauge;
            _chaosGaugeIcons[index].color = gaugeColor;
        }
    }

    private Sprite GetArtifactIcon(string artifactId)
    {
        if (_artifactIcons.TryGetValue(artifactId, out Sprite icon))
        {
            return icon;
        }

        icon = Resources.LoadAll<Sprite>("Sprites/Artifact/" + artifactId)
            .OrderByDescending(sprite => sprite.rect.width * sprite.rect.height)
            .First();
        _artifactIcons.Add(artifactId, icon);
        return icon;
    }

    private UnityEngine.UI.Image CreateIcon(Transform parent, string objectName, int size)
    {
        GameObject iconObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        iconObject.transform.SetParent(parent, false);
        UnityEngine.UI.Image icon = iconObject.GetComponent<UnityEngine.UI.Image>();
        RectTransform rectTransform = icon.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(size, size);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        return icon;
    }

    public void SelectArtifact(int index)
    {
        Npc.Artifacts.Select(_slots[index].ArtifactId);
    }

    public void Equip()
    {
        Npc.Artifacts.TryEquip(Npc.Artifacts.SelectedArtifact.Id);
    }

    public void Unequip()
    {
        Npc.Artifacts.Unequip(Npc.Artifacts.SelectedArtifact.Id);
    }

    protected override void CloseWindow()
    {
        UIManager.Instance.CloseArtifactUI();
    }
}
