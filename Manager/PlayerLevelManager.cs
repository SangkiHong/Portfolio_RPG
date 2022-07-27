using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SK
{
    public class PlayerLevelManager : MonoBehaviour
    {
        // 싱글톤
        public static PlayerLevelManager Instance { get; private set; }

        // 레벨업 시 호출될 이벤트
        public delegate void LevelUpHandler();
        public event LevelUpHandler OnLevelUp;

        [Header("Level Up")]
        [SerializeField] private CanvasGroup levelupCanvasGroup;
        [SerializeField] private Text levelupLevelText;
        [SerializeField] private float levelupUIAppearTime = 3;
        [SerializeField] private uint levelupBonusStat = 5;
        [SerializeField] private uint levelupBonusSkillPoint = 1;
        [SerializeField] private Button[] button_StatUp; // 0: STR, 1: DEX, 2: INT

        private UI.PlayerStateUIHandler _playerStateUIHandler;
        private Data.PlayerData _playerData;

        private uint _levelUpExp;
        public uint LevelUpExp => _levelUpExp;

        private void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            else
                Instance = this;
        }

        private void Start()
        {
            _playerStateUIHandler = UI.UIManager.Instance.playerStateUIHandler;
            _playerData = Data.DataManager.Instance.PlayerData;
            _levelUpExp = CalculateLevelUpExp();

            // 경험치 변경 시 호출될 이벤트 함수 등록
            Data.DataManager.Instance.OnChangedExp += CallbackUpdateExp;

            // 스탯 업 버튼 이벤트 등록
            for (int i = 0; i < button_StatUp.Length; i++)
            {
                int index = i;
                button_StatUp[i].onClick.AddListener(delegate { StatUp(index); });
            }
            // 잔여 스탯 포인트가 있다면 버튼 보이기
            if (_playerData.StatPoint > 0)
                StatUpButton(true);
        }

        public uint CalculateLevelUpExp()
        {
            uint level = _playerData.Level;
            uint targetExp = (uint)Mathf.FloorToInt(level * level * 1.2f * 30);
            // 레벨 끝자리가 4, 9인 경우 레벨구간 설정
            if (level % 5 == 4) targetExp += level * 50;

            return targetExp;
        }

        // 경험치 상승 콜백 함수(경험치 상태 업데이트)
        private void CallbackUpdateExp()
        {
            uint currentExp = _playerData.Exp;
            // 현재 경험치가 목표 경험치 이상인 경우 레벨 업
            if (currentExp >= _levelUpExp)
            {
                // 레벨 상승 및 레벨 표시 UI 업데이트
                _playerStateUIHandler.UpdateLevel(++_playerData.Level);

                // 잔여 경험치 넘김
                _playerData.Exp = currentExp - _levelUpExp;
                // 다음 목표 경험치 업데이트
                _levelUpExp = CalculateLevelUpExp();

                // 효과음 재생
                AudioManager.Instance.PlayAudio(Strings.Audio_FX_LevelUp);

                // 파티클 효과 재생
                EffectManager.Instance.PlayEffect(4000);

                // 레벨업 UI 표시
                StartCoroutine(ShowLevelUp());

                // 스탯 포인트 증가
                _playerData.StatPoint += levelupBonusStat;

                // 스킬 포인트 증가
                _playerData.SkillPoint += levelupBonusSkillPoint;

                // 스탯 업 표시
                StatUpButton(true);

                OnLevelUp?.Invoke();
            }

            // UI 표시 업데이트
            _playerStateUIHandler.UpdateExp();
        }

        // 스탯 업 이벤트 함수
        private void StatUp(int statIndex)
        {
            switch (statIndex)
            {
                case 0: // STR
                    _playerData.Str++;
                    break;
                case 1: // DEX
                    GameManager.Instance.Player.stamina.SetMaxSp(_playerData.Level, ++_playerData.Dex);
                    break;
                case 2: // INT
                    GameManager.Instance.Player.mana.SetMaxMp(_playerData.Level, ++_playerData.Int);
                    break;
            }

            // 공통적으로 최대 Hp 증가
            GameManager.Instance.Player.health.SetMaxHp(_playerData);

            // 스탯 차감 후 잔여 스탯이 없는 경우
            if (--_playerData.StatPoint == 0)
            {
                // 스탯 업 버튼 숨김
                StatUpButton(false);
            }

            // 플레이어 세부 정보 창 업데이트
            UI.UIManager.Instance.characterStatusManager.UpdateInformaion();
        }

        // 버튼 표시
        private void StatUpButton(bool isShow)
        {
            for (int i = 0; i < button_StatUp.Length; i++)
                button_StatUp[i].gameObject.SetActive(isShow);
        }

        // 레벨 업 UI 표시
        IEnumerator ShowLevelUp()
        {
            // 레벨 텍스트 할당
            levelupLevelText.text = _playerData.Level.ToString();

            float updateTime = 0.02f;
            WaitForSeconds ws = new WaitForSeconds(updateTime);

            // 레벨 업 UI 서서히 나타남
            while (levelupCanvasGroup.alpha < 1)
            {
                levelupCanvasGroup.alpha += updateTime;
                yield return ws;
            }

            float elapsed = 0;
            while (elapsed < levelupUIAppearTime)
            {
                elapsed += updateTime;
                yield return ws;
            }

            // 레벨 업 UI 서서히 사라짐
            while (levelupCanvasGroup.alpha > 0)
            {
                levelupCanvasGroup.alpha -= updateTime;
                yield return ws;
            }

            yield return null;
        }
    }
}