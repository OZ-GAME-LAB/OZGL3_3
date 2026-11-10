using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArtifactEquipment : SingletonBehaviour<PlayerArtifactEquipment>
{
    [SerializeField] private List<string> _ownedArtifacts = new List<string>();
    [SerializeField] private List<string> _equippedArtifacts = new List<string>();
    private readonly Dictionary<string, float> _equippedArtifactValues =
        new Dictionary<string, float>();

    public IReadOnlyList<string> OwnedArtifacts => _ownedArtifacts;
    public IReadOnlyList<string> EquippedArtifacts => _equippedArtifacts;

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;
    public event Action<string, float> OnArtifactEquipped;
    public event Action<string, float> OnArtifactUnequipped;

    private PlayerStatSet _playerStatSet;
    private PlayerStatus _playerStatus;
    private PlayerController _playerController;
    private PlayerCombatHandler _playerCombatHandler;

    private void Awake()
    {
        Init();
        _playerStatSet = GetComponent<PlayerStatSet>();
        _playerStatus = GetComponent<PlayerStatus>();
        _playerController = GetComponent<PlayerController>();
        _playerCombatHandler = GetComponent<PlayerCombatHandler>();
    }

    private void OnEnable()
    {
        if (_playerStatus != null)
        {
            _playerStatus.OnBlock += HandleBlock;
        }

        if (_playerController != null)
        {
            _playerController.OnCrouchStarted += HandleCrouchStarted;
        }
    }

    private void OnDisable()
    {
        if (_playerStatus != null)
        {
            _playerStatus.OnBlock -= HandleBlock;
        }

        if (_playerController != null)
        {
            _playerController.OnCrouchStarted -= HandleCrouchStarted;
        }
    }

    public bool LoadFromSave(UserArtifactData userArtifactData)
    {
        if (userArtifactData == null)
        {
            Logger.LogError("PlayerArtifactEquipment: UserArtifactData가 null입니다.");
            return false;
        }

        if (GameManager.Instance == null ||
            GameManager.DataTable == null ||
            GameManager.DataTable.Artifact == null)
        {
            Logger.LogError("PlayerArtifactEquipment: 아티팩트 데이터 테이블이 준비되지 않았습니다.");
            return false;
        }

        // 상점 구매와 대화 보상도 이 보유 목록을 갱신한다.
        _ownedArtifacts = userArtifactData.OwnedArtifactIds;

        bool success = true;
        HashSet<string> savedEquippedArtifacts =
            new HashSet<string>(userArtifactData.EquippedArtifactIds);

        for (int i = _equippedArtifacts.Count - 1; i >= 0; i--)
        {
            string artifactId = _equippedArtifacts[i];
            if (savedEquippedArtifacts.Contains(artifactId))
            {
                continue;
            }

            if (_equippedArtifactValues.TryGetValue(
                    artifactId,
                    out float value) &&
                !UnequipArtifact(artifactId, value))
            {
                success = false;
            }
        }

        foreach (string artifactId in userArtifactData.EquippedArtifactIds)
        {
            if (_equippedArtifacts.Contains(artifactId))
            {
                continue;
            }

            if (!_ownedArtifacts.Contains(artifactId))
            {
                Logger.LogWarning(
                    $"PlayerArtifactEquipment: 보유하지 않은 아티팩트는 장착하지 않습니다. ({artifactId})");
                success = false;
                continue;
            }

            if (!GameManager.DataTable.Artifact.TryGetValue(
                    artifactId,
                    out ArtifactData artifactData))
            {
                Logger.LogWarning(
                    $"PlayerArtifactEquipment: 아티팩트 데이터를 찾을 수 없습니다. ({artifactId})");
                success = false;
                continue;
            }

            if (!EquipArtifact(artifactId, artifactData.Value))
            {
                success = false;
            }
        }

        OnInventoryChanged?.Invoke();
        return success;
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

        if (!ApplyArtifactEffect(artifactId, artifactValue))
        {
            Logger.LogWarning("EquipArtifact: 아티팩트 효과를 적용하지 못했습니다.");
            return false;
        }

        _equippedArtifacts.Add(artifactId);
        _equippedArtifactValues[artifactId] = artifactValue;
        OnArtifactEquipped?.Invoke(artifactId, artifactValue);
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

        if (!RemoveArtifactEffect(artifactId, artifactValue))
        {
            Logger.LogWarning($"UnequipArtifact: 아티팩트 효과를 제거하지 못했습니다. " +
                $"(ID: {artifactId}, Value: {artifactValue})");
            return false;
        }

        _equippedArtifacts.Remove(artifactId);
        _equippedArtifactValues.Remove(artifactId);
        OnArtifactUnequipped?.Invoke(artifactId, artifactValue);
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

    private bool ApplyArtifactEffect(string artifactId, float value)
    {
        switch (artifactId)
        {
            case Const.MoveSpeedArtifactId:
                if (!TryGetPlayerStats())
                {
                    return false;
                }

                _playerStatSet.ApplyArtifactBonus(PlayerStatType.WalkSpeed, value);
                _playerStatSet.ApplyArtifactBonus(PlayerStatType.RunSpeed, value);
                return true;
            case Const.MaxHealthOneArtifactId:
            case Const.MaxHealthTwoArtifactId:
            case Const.MaxHealthThreeArtifactId:
                if (!TryGetPlayerStats())
                {
                    return false;
                }

                _playerStatSet.ApplyArtifactBonus(PlayerStatType.MaxHealth, value);
                return true;
            case Const.BlockHealArtifactId:
                return RequirePlayerStatus();
            case Const.BlockGaugeArtifactId:
                return RequirePlayerStatus();
            case Const.AttackGaugeArtifactId:
                if (_playerCombatHandler == null)
                {
                    Logger.LogError("PlayerArtifactEquipment: PlayerCombatHandler를 찾을 수 없습니다.");
                    return false;
                }

                _playerCombatHandler.SetAttackSkillGaugeGainBonus(
                    artifactId,
                    Mathf.RoundToInt(value));
                return true;
            case Const.SkillCostArtifactId:
                if (!RequirePlayerStatus())
                {
                    return false;
                }

                _playerStatus.SetSkillGaugeCostDecrease(artifactId, value);
                return true;
            case Const.DashCooldownArtifactId:
                if (_playerController == null)
                {
                    Logger.LogError("PlayerArtifactEquipment: PlayerController를 찾을 수 없습니다.");
                    return false;
                }

                _playerController.SetDashCooldownDecrease(value);
                return true;
            case Const.CrouchAttackArtifactId:
            case Const.BlockAttackArtifactId:
                return TryGetPlayerStats();
            case Const.ParryArtifactId:
                Logger.LogWarning(
                    "PlayerArtifactEquipment: 패링 아티팩트는 패링 규칙 확정 후 연결해야 합니다.");
                return true;
            default:
                Logger.LogWarning(
                    $"PlayerArtifactEquipment: 아직 지원하지 않는 아티팩트입니다. ({artifactId})");
                return false;
        }
    }

    private bool RemoveArtifactEffect(string artifactId, float value)
    {
        switch (artifactId)
        {
            case Const.MoveSpeedArtifactId:
                if (!TryGetPlayerStats())
                {
                    return false;
                }

                _playerStatSet.ApplyArtifactBonus(PlayerStatType.WalkSpeed, -value);
                _playerStatSet.ApplyArtifactBonus(PlayerStatType.RunSpeed, -value);
                return true;
            case Const.MaxHealthOneArtifactId:
            case Const.MaxHealthTwoArtifactId:
            case Const.MaxHealthThreeArtifactId:
                if (!TryGetPlayerStats())
                {
                    return false;
                }

                _playerStatSet.ApplyArtifactBonus(PlayerStatType.MaxHealth, -value);
                return true;
            case Const.BlockHealArtifactId:
            case Const.BlockGaugeArtifactId:
                return RequirePlayerStatus();
            case Const.AttackGaugeArtifactId:
                if (_playerCombatHandler == null)
                {
                    Logger.LogError("PlayerArtifactEquipment: PlayerCombatHandler를 찾을 수 없습니다.");
                    return false;
                }

                _playerCombatHandler.RemoveAttackSkillGaugeGainBonus(artifactId);
                return true;
            case Const.SkillCostArtifactId:
                if (!RequirePlayerStatus())
                {
                    return false;
                }

                _playerStatus.RemoveSkillGaugeCostReduction(artifactId);
                return true;
            case Const.DashCooldownArtifactId:
                if (_playerController == null)
                {
                    Logger.LogError("PlayerArtifactEquipment: PlayerController를 찾을 수 없습니다.");
                    return false;
                }

                _playerController.RemoveDashCooldownDecrease();
                return true;
            case Const.CrouchAttackArtifactId:
            case Const.BlockAttackArtifactId:
                if (!TryGetPlayerStats())
                {
                    return false;
                }

                _playerStatSet.RemoveVariableAttackDamageBonus(artifactId);
                return true;
            case Const.ParryArtifactId:
                return true;
            default:
                Logger.LogWarning(
                    $"PlayerArtifactEquipment: 아직 지원하지 않는 아티팩트입니다. ({artifactId})");
                return false;
        }
    }

    private void HandleBlock()
    {
        if (_equippedArtifactValues.TryGetValue(
                Const.BlockHealArtifactId,
                out float healChance) &&
            UnityEngine.Random.value < healChance)
        {
            _playerStatus.Heal(1);
        }

        if (_equippedArtifactValues.TryGetValue(
                Const.BlockGaugeArtifactId,
                out float gaugeValue))
        {
            int amount = Mathf.RoundToInt(gaugeValue);
            if (amount > 0)
            {
                _playerStatus.AddSkillGauge(amount);
            }
        }

        if (_equippedArtifactValues.TryGetValue(
                Const.BlockAttackArtifactId,
                out float attackBonus))
        {
            _playerStatSet.SetVariableAttackDamageBonus(
                Const.BlockAttackArtifactId,
                Mathf.RoundToInt(attackBonus),
                1f);
        }
    }

    private void HandleCrouchStarted()
    {
        if (_equippedArtifactValues.TryGetValue(
                Const.CrouchAttackArtifactId,
                out float attackBonus))
        {
            _playerStatSet.SetVariableAttackDamageBonus(
                Const.CrouchAttackArtifactId,
                Mathf.RoundToInt(attackBonus),
                1f);
        }
    }

    private bool TryGetPlayerStats()
    {
        if (_playerStatSet != null)
        {
            return true;
        }

        Logger.LogError("PlayerArtifactEquipment: PlayerStatSet을 찾을 수 없습니다.");
        return false;
    }

    private bool RequirePlayerStatus()
    {
        if (_playerStatus != null)
        {
            return true;
        }

        Logger.LogError("PlayerArtifactEquipment: PlayerStatus를 찾을 수 없습니다.");
        return false;
    }
}
