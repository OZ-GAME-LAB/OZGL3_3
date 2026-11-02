using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArtifactEquipment : SingletonBehaviour<PlayerArtifactEquipment>
{
    // TODO(이주헌): 실제 아티팩트 데이터 구조 연결 후 변경
    [SerializeField] private List<string> _ownedArtifacts = new List<string>();
    [SerializeField] private List<string> _equippedArtifacts = new List<string>();

    public IReadOnlyList<string> OwnedArtifacts => _ownedArtifacts;
    public IReadOnlyList<string> EquippedArtifacts => _equippedArtifacts;

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

    private void Awake()
    {
        Init();
    }

    public bool GetArtifact(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogError("GetArtifact: 아티팩트 ID가 비어 있습니다.");

            return false;
        }

        if (_ownedArtifacts.Contains(artifactId))
        {
            Logger.LogError("GetArtifact: 아티팩트 ID가 일치하지 않습니다.");

            return false;
        }

        _ownedArtifacts.Add(artifactId);
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool EquipArtifact(string artifactId, float artifactValue)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogWarning("EquipArtifact: 아티팩트 ID가 비어 있습니다.");

            return false;
        }

        if (!_ownedArtifacts.Contains(artifactId) || _equippedArtifacts.Contains(artifactId))
        {
            Logger.LogWarning("EquipArtifact: 아티팩트 ID가 일치하지 않습니다.");

            return false;
        }

        if (!ApplyArtifactBonus(artifactId, artifactValue, 1))
        {
            Logger.LogWarning("EquipArtifact: 아티팩트 보너스 회수에 실패했습니다.");

            return false;
        }

        _equippedArtifacts.Add(artifactId);
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool UnequipArtifact(string artifactId, float artifactValue)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogWarning("UnequipArtifact: 아티팩트 ID가 비어 있습니다.");

            return false;
        }

        if (!_equippedArtifacts.Contains(artifactId))
        {
            Logger.LogWarning("UnequipArtifact: 아티팩트 ID가 일치하지 않습니다.");

            return false;
        }

        if (!ApplyArtifactBonus(artifactId, artifactValue, - 1))
        {
            Logger.LogWarning($"UnequipArtifact: 아티팩트 보너스 회수에 실패했습니다. " +
                $"(ID: {artifactId}, Value: {artifactValue})");

            return false;
        }

        _equippedArtifacts.Remove(artifactId);
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool RemoveArtifact(string artifactId, float artifactValue)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Logger.LogWarning("RemoveArtifact: 아티팩트 ID가 비어 있습니다.");
            return false;
        }

        if (!_ownedArtifacts.Contains(artifactId))
        {
            Logger.LogWarning("RemoveArtifact: 아티팩트 ID가 일치하지 않습니다.");

            return false;
        }

        if (_equippedArtifacts.Contains(artifactId))
        {
            if (!UnequipArtifact(artifactId, artifactValue))
            {
                Logger.LogWarning("RemoveArtifact: 아티팩트 회수에 실패했습니다.");

                return false;
            }
        }

        _ownedArtifacts.Remove(artifactId);
        OnInventoryChanged?.Invoke();

        return true;
    }

    private bool ApplyArtifactBonus(string artifactId, float value, int apply)
    {
        if (!PlayerStatSet.TryParseStatType(artifactId, out PlayerStatType statType))
        {
            Logger.LogWarning($"ApplyArtifactBonus: 등록되지 않은 아티팩트 ID입니다. ({artifactId})");
            return false;
        }

        PlayerStatSet.Instance.ApplyArtifactBonus(statType, value * apply);
        return true;
    }
}
