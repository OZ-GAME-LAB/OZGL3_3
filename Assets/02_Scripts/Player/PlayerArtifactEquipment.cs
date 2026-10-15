using System;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class PlayerArtifactEquipment : MonoBehaviour
{
    [SerializeField] private List<string> _ownedArtifacts = new List<string>();
    [SerializeField] private List<string> _equippedArtifacts = new List<string>();

    private PlayerStats _playerStats;

    public IReadOnlyList<string> OwnedArtifacts => _ownedArtifacts;
    public IReadOnlyList<string> EquippedArtifacts => _equippedArtifacts;

    private void Awake()
    {
        if (_playerStats == null)
        {
            _playerStats = GetComponent<PlayerStats>();
        }
    }

    //TODO(이주헌) : 인벤토리 및 아티팩트 구현 후 주석 해제, List<string> 은 임시
    //public int CurrentChaosGauge
    //{
    //    get
    //    {
    //        int currentChaosGauge = 0;

    //        foreach (string artifact in _equippedArtifacts)
    //        {
    //            if (artifact != null)
    //                currentChaosGauge += artifact.ChaosGauge;
    //        }
    //        return currentChaosGauge;
    //    }
    //}

    //public int MaxChaosGauge
    //{
    //    get
    //    {
    //        int maxChaosGauge = _playerStats.MaxChaosGauge

    //        foreach (string artifact in _equippedArtifacts)
    //        {
    //            if (artifact != null)
    //                maxChaosGauge += artifact.MaxChaosGaugeBonusValue;
    //        }
    //        return maxChaosGauge;
    //    }
    //}

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

    public bool GainArtifact(string artifact)
    {
        if (artifact == null)
        {
            Debug.LogError("AcquireArtifact: 아티팩트가 null입니다.", this);
            return false;
        }

        if (_ownedArtifacts.Contains(artifact))
        {
            return false;
        }

        _ownedArtifacts.Add(artifact);
        OnInventoryChanged?.Invoke();
        return true;
    }

    //public bool EquipArtifact(string artifact)
    //{
    //    if (artifact == null)
    //    {
    //        Debug.LogError("EquipArtifact: 아티팩트가 null입니다.", this);
    //        return false;
    //    }

    //    if (!_ownedArtifacts.Contains(artifact) || _equippedArtifacts.Contains(artifact))
    //    {
    //        return false;
    //    }

    //    if (CurrentChaosGauge + artifact.ChaosGaugeValue > MaxChaosGauge)
    //    {
    //        Debug.LogWarning($"카오스 게이지가 최대치 {MaxChaosGauge}을 초과합니다.", this);
    //        return false;
    //    }

    //    _equippedArtifacts.Add(artifact);
    //    OnEquipmentChanged?.Invoke();
    //    return true;
    //}

    public bool UnequipArtifact(string artifact)
    {
        if (artifact == null)
        {
            Debug.LogError("UnequipArtifact: 아티팩트가 null입니다.", this);
            return false;
        }

        if (!_equippedArtifacts.Remove(artifact))
        {
            return false;
        }

        OnEquipmentChanged?.Invoke();
        return true;
    }

    public bool RemoveArtifact(string artifact)
    {
        if (artifact == null)
        {
            Debug.LogError("RemoveArtifact: 아티팩트가 null입니다.", this);
            return false;
        }

        if (!_ownedArtifacts.Contains(artifact))
        {
            return false;
        }

        bool wasEquipped = _equippedArtifacts.Remove(artifact);
        _ownedArtifacts.Remove(artifact);
        OnInventoryChanged?.Invoke();

        if (wasEquipped)
        {
            OnEquipmentChanged?.Invoke();
        }

        return true;
    }
}
