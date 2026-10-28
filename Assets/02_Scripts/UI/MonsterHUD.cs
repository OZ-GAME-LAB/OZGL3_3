using UnityEngine;
using UnityEngine.UI;

// 몬스터 체력바: 몬스터 머리 위에 떠 있는 월드 스페이스 슬라이더. 맞아서 체력이 줄면 보이고, 죽으면 숨긴다.
public class MonsterHUD : MonoBehaviour
{
    [SerializeField] private GameObject Object_BarRoot;    // 켜고 끌 자식 오브젝트
    [SerializeField] private Slider Slider_Hp;             // Min 0, Max 1
    [SerializeField] private bool _isHideWhenFull = true;

    private MonsterBase _monster;

    private void Awake()
    {
        _monster = GetComponentInParent<MonsterBase>();

        if (_monster == null)
        {
            Logger.LogError("MonsterHpBar: MonsterBase를 찾지 못했습니다.");
        }
    }

    private void Start()
    {
        if (_monster == null)
        {
            return;
        }

        _monster.OnHpChanged += OnChangedHp;

        // 몬스터는 풀피로 시작한다. 현재값 getter가 없으므로 첫 이벤트가 오기 전까지 풀피로 그린다.
        Slider_Hp.value = 1f;
        Object_BarRoot.SetActive(_isHideWhenFull == false);
    }

    private void OnDestroy()
    {
        if (_monster != null)
        {
            _monster.OnHpChanged -= OnChangedHp;
        }
    }

    // 몬스터가 Flip(localScale.x 반전)해도 체력바는 뒤집히지 않게 되돌린다.
    private void LateUpdate()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(transform.parent.lossyScale.x);
        transform.localScale = scale;
    }

    private void OnChangedHp(float current, float max)
    {
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        Slider_Hp.value = ratio;

        bool isVisible = ratio > 0f && (_isHideWhenFull == false || ratio < 1f);
        Object_BarRoot.SetActive(isVisible);
    }
}