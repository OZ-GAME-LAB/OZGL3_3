using UnityEngine;

public abstract class PlayerSkillBase
{
    private readonly string _skillId;
    private readonly PlayerStatus _playerStatus;
    private readonly Animator _animator;

    protected PlayerSkillData SkillData { get; private set; }

    public bool IsSkillActive { get; private set; }

    private bool _isEffectApplied;

    protected PlayerSkillBase(string skillId, PlayerStatus playerStatus, Animator animator)
    {
        _skillId = skillId;
        _playerStatus = playerStatus;
        _animator = animator;
    }

    public bool TryUse()
    {
        if (_playerStatus == null || _animator == null || _playerStatus.IsDead || IsSkillActive)
        {
            return false;
        }

        if (GameManager.Instance == null ||
            !GameManager.DataTable.TryGetPlayerSkill(_skillId, out PlayerSkillData skillData))
        {
            Logger.LogError($"스킬 데이터를 찾을 수 없습니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(skillData.TriggerName))
        {
            Logger.LogError($"스킬 TriggerName이 비어 있습니다. ({_skillId})");
            return false;
        }

        if (!CanUseSkill(skillData))
        {
            return false;
        }

        if (!_playerStatus.TryUseSkillGauge(skillData.needGauge))
        {
            return false;
        }

        SkillData = skillData;
        _isEffectApplied = false;
        IsSkillActive = true;
        _animator.SetTrigger(skillData.TriggerName.Trim());
        return true;
    }

    public void AnimationEffect()
    {
        if (!IsSkillActive || _isEffectApplied)
        {
            return;
        }

        ApplyEffect(SkillData);
        _isEffectApplied = true;
    }

    public void AnimationEnd()
    {
        IsSkillActive = false;
        _isEffectApplied = false;
        SkillData = null;
    }

    public void Cancel()
    {
        if (IsSkillActive && _animator != null && SkillData != null)
        {
            _animator.ResetTrigger(SkillData.TriggerName.Trim());
        }

        AnimationEnd();
    }

    protected abstract bool CanUseSkill(PlayerSkillData skillData);

    protected abstract void ApplyEffect(PlayerSkillData skillData);

    protected PlayerStatus PlayerStatus => _playerStatus;
}
