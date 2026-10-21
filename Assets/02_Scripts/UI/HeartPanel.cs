using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 체력 하트: 최대 체력만큼 하트를 만들고, 현재 체력만큼 채운다. (나머지는 빈 하트)
public class HeartPanel : MonoBehaviour
{
    [SerializeField] private Transform Transform_HeartRoot;
    [SerializeField] private Image _heartPrefab;
    [SerializeField] private Sprite _sprite_HeartFull;
    [SerializeField] private Sprite _sprite_HeartEmpty;

    private readonly List<Image> _hearts = new List<Image>();
    private PlayerStatus _playerStatus;

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 구독을 반드시 해제한다.
        DoUnsubscribe();
        _playerStatus = null;
    }

    public void Init(PlayerStatus playerStatus)
    {
        DoUnsubscribe();

        if (playerStatus == null)
        {
            Logger.LogError("HeartPanel: PlayerStatus가 null입니다.");
            return;
        }

        _playerStatus = playerStatus;
        _playerStatus.OnHealthChanged += OnChangedHealth;

        // 이벤트는 값이 바뀔 때만 오므로, 시작할 때 현재 값을 직접 한 번 그린다.
        DoRefresh(_playerStatus.CurrentHealth, _playerStatus.MaxHealth);
    }

    private void DoUnsubscribe()
    {
        if (_playerStatus != null)
        {
            _playerStatus.OnHealthChanged -= OnChangedHealth;
        }
    }

    private void OnChangedHealth(int current, int max)
    {
        DoRefresh(current, max);
    }

    private void DoRefresh(int current, int max)
    {
        // 최대 체력이 늘면 하트를 더 만들고, 남는 하트는 숨긴다.
        while (_hearts.Count < max)
        {
            _hearts.Add(Instantiate(_heartPrefab, Transform_HeartRoot));
        }

        for (int i = 0; i < _hearts.Count; i++)
        {
            bool isVisible = i < max;
            _hearts[i].gameObject.SetActive(isVisible);

            if (isVisible == true)
            {
                _hearts[i].sprite = i < current ? _sprite_HeartFull : _sprite_HeartEmpty;
            }
        }
    }
}