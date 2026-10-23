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
        switch (artifactId)
        {
            case "JumpValue":
                PlayerStats.Instance.ApplyArtifactBonus("JumpValue", value * apply);
                break;

            case "WalkSpeed":
                PlayerStats.Instance.ApplyArtifactBonus("WalkSpeed", value * apply);
                break;

            case "RunSpeed":
                PlayerStats.Instance.ApplyArtifactBonus("RunSpeed", value * apply);
                break;

            case "CrouchSpeed":
                PlayerStats.Instance.ApplyArtifactBonus("CrouchSpeed", value * apply);
                break;

            case "LadderSpeed":
                PlayerStats.Instance.ApplyArtifactBonus("LadderSpeed", value * apply);
                break;

            case "AttackDamage":
                PlayerStats.Instance.ApplyArtifactBonus("AttackDamage", value * apply);
                break;

            case "MaxDashChance":
                PlayerStats.Instance.ApplyArtifactBonus("MaxDashChance", value * apply);
                break;

            case "MaxHealth":
                PlayerStats.Instance.ApplyArtifactBonus("MaxHealth", value * apply);
                break;

            case "MaxSkillGauge":
                PlayerStats.Instance.ApplyArtifactBonus("MaxSkillGauge", value * apply);
                break;

            case "MaxChaosGauge":
                PlayerStats.Instance.ApplyArtifactBonus("MaxChaosGauge", value * apply);
                break;

            default:
                Logger.LogWarning($"ApplyArtifactBonus: 등록되지 않은 아티팩트 ID입니다. ({artifactId})");
                return false;
        }

        return true;
    }
}
