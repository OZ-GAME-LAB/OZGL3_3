using System;
using System.Collections.Generic;
using System.Linq;

public class ArtifactSlotState
{
    public string ArtifactId { get; }
    public ArtifactData Artifact { get; }
    public bool IsOwned => Artifact != null;
    public bool IsEquipped { get; }

    public ArtifactSlotState(string artifactId, ArtifactData artifact, bool isEquipped)
    {
        ArtifactId = artifactId;
        Artifact = artifact;
        IsEquipped = isEquipped;
    }
}

public class ArtifactService
{
    private const int _maxChaosGauge = 10;
    private const int _forcedEquipAttempts = 5;
    private readonly DataTableManager _tables;
    private readonly UserArtifactData _userArtifactData;
    private string _rejectedArtifactId;
    private int _rejectedAttempts;

    public IReadOnlyCollection<string> OwnedArtifactIds => _userArtifactData.OwnedArtifactIds;
    public IReadOnlyList<string> EquippedArtifactIds => _userArtifactData.EquippedArtifactIds;
    public int MaxChaosGauge => _maxChaosGauge;
    public int CurrentChaosGauge => EquippedArtifactIds.Sum(id => _tables.Artifact[id].ChaosCost);
    public bool IsOverloaded => CurrentChaosGauge > MaxChaosGauge;
    public bool IsOverloadUnlocked => _userArtifactData.IsOverloadUnlocked;
    public int RejectedAttempts => _rejectedAttempts;
    public ArtifactData SelectedArtifact { get; private set; }

    public event Action OnChanged;
    public event Action OnEquipRejected;

    public ArtifactService(DataTableManager tables, UserDataManager userData)
    {
        _tables = tables;
        _userArtifactData = userData.GetUserData<UserArtifactData>();
        if (IsOverloaded)
        {
            _userArtifactData.IsOverloadUnlocked = true;
        }
    }

    public IEnumerable<ArtifactSlotState> GetSlots()
    {
        return _tables.Artifact.Values.OrderBy(artifact => artifact.Id, StringComparer.Ordinal)
            .Select(artifact => new ArtifactSlotState(
                artifact.Id,
                _userArtifactData.OwnedArtifactIds.Contains(artifact.Id) ? artifact : null,
                _userArtifactData.EquippedArtifactIds.Contains(artifact.Id)));
    }

    public bool Select(string artifactId)
    {
        if (!_userArtifactData.OwnedArtifactIds.Contains(artifactId))
        {
            return false;
        }

        if (SelectedArtifact?.Id != artifactId)
        {
            ResetAttempts();
        }

        SelectedArtifact = _tables.Artifact[artifactId];
        OnChanged?.Invoke();
        return true;
    }

    public bool GainArtifact(string artifactId)
    {
        if (!_tables.Artifact.ContainsKey(artifactId) || _userArtifactData.OwnedArtifactIds.Contains(artifactId))
        {
            return false;
        }

        _userArtifactData.OwnedArtifactIds.Add(artifactId);
        _userArtifactData.SaveData();
        OnChanged?.Invoke();
        return true;
    }

    public void TryEquip(string artifactId)
    {
        if (_rejectedArtifactId != artifactId)
        {
            ResetAttempts();
        }

        if (!_userArtifactData.OwnedArtifactIds.Contains(artifactId))
        {
            return;
        }

        if (_userArtifactData.EquippedArtifactIds.Contains(artifactId))
        {
            ResetAttempts();
            return;
        }

        ArtifactData artifact = _tables.Artifact[artifactId];
        if (artifact.ChaosCost < 0)
        {
            ResetAttempts();
            return;
        }

        if (CurrentChaosGauge >= MaxChaosGauge)
        {
            ResetAttempts();
            OnEquipRejected?.Invoke();
            return;
        }

        bool isForced = (long)CurrentChaosGauge + artifact.ChaosCost > MaxChaosGauge;
        if (isForced && !IsOverloadUnlocked)
        {
            _rejectedArtifactId = artifactId;
            _rejectedAttempts++;
            if (_rejectedAttempts < _forcedEquipAttempts)
            {
                OnEquipRejected?.Invoke();
                OnChanged?.Invoke();
                return;
            }
        }

        _userArtifactData.EquippedArtifactIds.Add(artifactId);
        if (isForced)
        {
            _userArtifactData.IsOverloadUnlocked = true;
        }

        ResetAttempts();
        _userArtifactData.SaveData();
        OnChanged?.Invoke();
        return;
    }

    public bool Unequip(string artifactId)
    {
        if (!_userArtifactData.EquippedArtifactIds.Remove(artifactId))
        {
            return false;
        }

        ResetAttempts();
        _userArtifactData.SaveData();
        OnChanged?.Invoke();
        return true;
    }

    public bool RemoveArtifact(string artifactId)
    {
        if (!_userArtifactData.OwnedArtifactIds.Remove(artifactId))
        {
            return false;
        }

        _userArtifactData.EquippedArtifactIds.Remove(artifactId);

        if (SelectedArtifact?.Id == artifactId)
        {
            SelectedArtifact = null;
        }

        ResetAttempts();
        _userArtifactData.SaveData();
        OnChanged?.Invoke();
        return true;
    }

    public void ResetAttempts()
    {
        _rejectedArtifactId = null;
        _rejectedAttempts = 0;
    }
}
