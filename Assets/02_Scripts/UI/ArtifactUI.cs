using System;
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

    protected override NpcType ScreenType => NpcType.Artifact;

    protected override void OnEnable()
    {
        base.OnEnable();
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
            _slotButtons[index].SetInteractable(exists && _slots[index].IsOwned);
            _slotButtons[index].ChangeButtonText(exists && _slots[index].IsOwned
                ? _slots[index].Artifact.Name : string.Empty);
        }

        _equipmentText.text = "장착: " + string.Join(" / ", Npc.Artifacts.EquippedArtifactIds.Select(id => Npc.Tables.Artifact[id].Name));
        _equipmentText.color = Npc.EquipmentTint;
        
        _gaugeText.text = $"혼돈 {Npc.Artifacts.CurrentChaosGauge}/{Npc.Artifacts.MaxChaosGauge}";
        _gaugeText.color = Npc.EquipmentTint;

        ArtifactData selected = Npc.Artifacts.SelectedArtifact;
        bool equipped = selected != null && Npc.Artifacts.EquippedArtifactIds.Contains(selected.Id);
        bool hasCost = selected != null && selected.ChaosCost >= 0;
        _equipButton.SetInteractable(hasCost && !equipped
            && Npc.Artifacts.CurrentChaosGauge < Npc.Artifacts.MaxChaosGauge);
        _unequipButton.SetInteractable(equipped);
        _descriptionText.text = selected == null ? "소유한 아티펙트를 선택하세요."
            : $"{selected.Name}\n\n{selected.Desc}\n\n혼돈: {(hasCost ? selected.ChaosCost.ToString() : "미설정")}";
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
