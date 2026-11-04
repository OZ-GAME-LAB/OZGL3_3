using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Cooldown;   // 검정(Alpha ~150) / Filled / Radial 360 / Origin Top
    [SerializeField] private Color _readyColor = Color.white;
    [SerializeField] private Color _lackGaugeColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    private string _skillId;

    public string SkillId => _skillId;
    public void SetSkillId(string skillId) { _skillId = skillId; }

    private PlayerController _controller;
    private PlayerStatus _status;

    private int _needGauge;
    private float _duration;
    private bool _onCooldown;

    public void SetIcon(Sprite icon)
    {
        if (icon != null)
        {
            Image_Icon.sprite = icon;
        }
    }

    public void Init(string skillId, PlayerController controller, PlayerStatus status)
    {
        Release();

        _skillId = skillId;
        _controller = controller;
        _status = status;

        if (GameManager.DataTable.TryGetPlayerSkill(skillId, out PlayerSkillData data))
        {
            _needGauge = data.needGauge;
        }

        Image_Cooldown.type = Image.Type.Filled;
        Image_Cooldown.fillMethod = Image.FillMethod.Radial360;
        Image_Cooldown.fillOrigin = (int)Image.Origin360.Top;
        Image_Cooldown.fillClockwise = false;

        _controller.OnSkillCooldownStarted += HandleCooldownStarted;
        _controller.OnSkillCooldownEnded += HandleCooldownEnded;
        _status.OnSkillGaugeChanged += HandleGaugeChanged;

        SetReady();
    }

    public void Release()
    {
        if (_controller != null)
        {
            _controller.OnSkillCooldownStarted -= HandleCooldownStarted;
            _controller.OnSkillCooldownEnded -= HandleCooldownEnded;
        }

        if (_status != null)
        {
            _status.OnSkillGaugeChanged -= HandleGaugeChanged;
        }

        _controller = null;
        _status = null;
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Update()
    {
        if (_onCooldown == false || _controller == null)
        {
            return;
        }

        float remaining = GetRemaining();
        if (remaining <= 0f)
        {
            SetReady();
            return;
        }

        // 가득(검정) → 0(투명)으로 줄어들며 시계방향으로 걷힌다.
        Image_Cooldown.fillAmount = Mathf.Clamp01(remaining / _duration);
    }

    private float GetRemaining()
    {
        if (_skillId == Const.HealSkillId)
        {
            return _controller.HealSkillCooldownRemaining;
        }
        return _controller.GuardedAttackSkillCooldownRemaining;
    }

    private void HandleCooldownStarted(string skillId, float duration)
    {
        if (skillId != _skillId)
        {
            return;
        }

        _duration = Mathf.Max(duration, GetRemaining(), 0.01f);
        _onCooldown = true;
        Image_Cooldown.enabled = true;
        Image_Cooldown.fillAmount = 1f;
    }

    private void HandleCooldownEnded(string skillId)
    {
        if (skillId == _skillId)
        {
            SetReady();
        }
    }

    private void HandleGaugeChanged(int current, int max)
    {
        Image_Icon.color = current >= _needGauge ? _readyColor : _lackGaugeColor;
    }

    private void SetReady()
    {
        _onCooldown = false;
        Image_Cooldown.fillAmount = 0f;
        Image_Cooldown.enabled = false;
    }
}