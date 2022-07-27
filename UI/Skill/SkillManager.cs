using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Data;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 스킬 창 UI 및 스킬 데이터 관리자 클래스
     * 작성일: 22년 6월 19일
     */

    public class SkillManager : MonoBehaviour
    {
        [Header("Component")]
        // 스킬 세부 정보 패널
        [SerializeField] private SkillSpecificsPanel skillSpecificsPanel;
        // 스킬 슬롯의 부모 트랜스폼
        [SerializeField] private Transform skillTreeParent;

        [Header("UI")]
        [SerializeField] private Text text_SkillPoint;

        // 레퍼런스
        private FSM.Player _player;
        private QuickSlotManager quickSlotManager;

        // 슬롯을 저장할 배열
        private SkillSlot[] _skillSlots;
        // 스킬 슬롯을 저장할 딕셔너리(키: 스킬ID, 값: 스킬슬롯)
        private Dictionary<int, SkillSlot> _skillSlotDic;

        // 쿨타임 시간체크용 배열(슬롯 인덱스로 접근)
        private float[] _coolTimeArr;

        // 프로퍼티
        public IReadOnlyDictionary<int, SkillSlot> SkillSlotDic => _skillSlotDic;

        private void Awake()
        {
            // 슬롯들을 가져와 배열에 저장
            _skillSlots = skillTreeParent.GetComponentsInChildren<SkillSlot>();

            // 딕셔너리 초기화
            _skillSlotDic = new Dictionary<int, SkillSlot>();
            _coolTimeArr = new float[_skillSlots.Length];

            // 데이터 할당
            for (int i = 0; i < _skillSlots.Length; i++)
            {
                // 슬롯 ID 할당
                _skillSlots[i].SetSlotID(i);

                var skillID = _skillSlots[i].SkillData.skillID;

                // 딕셔너리에 데이터 추가
                _skillSlotDic.Add(skillID, _skillSlots[i]);

                // 이벤트 등록
                _skillSlots[i].OnMouseLeftClick += OnSlotLeftClick;
                _skillSlots[i].OnMouseRightClick += OnSlotRightClick;
                _skillSlots[i].OnMouseEnter += OnPointerEnter;
                _skillSlots[i].OnMouseExit += OnPointerExit;
                _skillSlots[i].OnDragEndEvent += OnDragEnd;
            }

            // 스킬 연결
            for (int i = 0; i < _skillSlots.Length; i++)
            {
                // 다음 스킬 데이터가 NULL이 아닌 경우 다음 스킬 데이터와 연결
                if (_skillSlots[i].SkillData.nextSkill != null && _skillSlots[i].SkillData.nextSkill.prevSkill == null)
                    _skillSlots[i].SkillData.nextSkill.prevSkill = _skillSlots[i].SkillData;
            }
        }

        // 데이터에 근거해 스킬 트리 활성화
        public void Initialize()
        {
            // 레퍼런스 변수로 할당
            _player = GameManager.Instance.Player;
            quickSlotManager = UI.UIManager.Instance.quickSlotManager;

            // 데이터 불러온 후 스킬 슬롯 활성화
            GameManager.Instance.DataManager.LoadSKill(ref _skillSlotDic);

            // 씬 매니저의 업데이트에 업데이트 함수 할당
            SceneManager.Instance.OnFixedUpdate += FixedTick;

            // 스킬 포인트 표시
            UpdateSkillPointInfoText();

            // 레벨 업 시 호출될 이벤트 함수 할당
            PlayerLevelManager.Instance.OnLevelUp += UpdateSkillPointInfoText;
        }

        // 타이머를 업데이트하는 함수
        private void FixedTick()
        {
            for (int i = 0; i < _coolTimeArr.Length; i++)
                if (_coolTimeArr[i] > 0)
                {
                    _coolTimeArr[i] -= Time.fixedDeltaTime;
                    // 스킬 창의 쿨타임 표시
                    _skillSlots[i].UpdateCoolTime(_coolTimeArr[i]);
                    // 퀵 슬롯에 할당된 스킬의 쿨타임 표시
                    quickSlotManager.GetSlotBySkill(_skillSlots[i].SkillData)?.UpdateCoolTime(_coolTimeArr[i]);
                }
        }

        // 스킬 사용 함수
        public void UseSkill(SkillData skillData)
        {
            // 인터렉팅 상태인 경우 리턴
            if (_player.anim.GetBool(Strings.animPara_isInteracting)) return;

            var slotID = SkillSlotDic[skillData.skillID].slotID;
            if (_coolTimeArr[slotID] <= 0)
            {
                // 사용 MP가 부족한 경우
                if (!_player.mana.UseMp(skillData.useMpAmount))
                    return;

                Debug.Log($"스킬 {skillData.skillName}을 사용");
                _coolTimeArr[slotID] = skillData.skillCoolTime;

                // 전투 모드가 아닌 경우 즉시 장비 착용
                if (!_player.onCombatMode)
                    _player.ImmediatelyEquipWeapon();

                _player.combat.BeginAttack(skillData.skillAttack);
                _player.stateMachine.ChangeState(_player.stateMachine.attackState);
            }
            else
            {
                // TODO: MP 부족 안내 UI 표시
                Debug.Log($"{skillData.skillName} 스킬이 현재 쿨타임 중... {_coolTimeArr[slotID]} 초 남음");
            }
        }

        // 스킬 포인트 UI 업데이트
        private void UpdateSkillPointInfoText()
        {
            uint usedPoint = 0;
            for (int i = 0; i < _skillSlots.Length; i++)
            {
                if (_skillSlots[i].IsActivated)
                    usedPoint += _skillSlots[i].SkillData.requireSkillPoint;
            }

            uint totalPoint = GameManager.Instance.DataManager.PlayerData.Level - 1;

            text_SkillPoint.text = (totalPoint - usedPoint) + Strings.QuestMiniInfo_Slash + totalPoint;
        }

        // 스킬 활성화
        private void ActivateSkill(SkillSlot skillSlot)
        {
            // 잔여 스킬 포인트 확인
            if (skillSlot.SkillData.requireSkillPoint <= GameManager.Instance.DataManager.PlayerData.SkillPoint)
            {
                SkillData prevSkill = skillSlot.SkillData.prevSkill;
                // 스킬 트리의 이전 스킬이 확성화가 안된 경우
                if (prevSkill != null && !_skillSlotDic[prevSkill.skillID].IsActivated)
                    return;

                // 스킬 포인트 사용 처리 시도
                if (GameManager.Instance.DataManager.UseSkillPoint(skillSlot.SkillData.requireSkillPoint))
                {
                    // 스킬 포인트 사용이 완료되면 스킬 개발
                    skillSlot.Active();

                    // 잔여 스킬 포인트 표시 업데이트
                    UpdateSkillPointInfoText();

                    // 스킬 세부 정보 창 표시 업데이트
                    skillSpecificsPanel.UpdateSkillState();
                }
            }
        }

        // 스킬 활성화 여부 반환
        public bool IsActivated(SkillData skillData)
        {
            return _skillSlotDic[skillData.skillID].IsActivated;
        }

        #region Event
        private void OnSlotLeftClick(int slotID)
        {
            // 활성화되지 않은 스킬인 경우
            if (_skillSlots[slotID].IsActivated)
                quickSlotManager.AssignSkill(_skillSlots[slotID].SkillData, true);
        }
        private void OnSlotRightClick(int slotID)
        {
            SkillSlot selectedSlot = _skillSlots[slotID];

            // 활성화되지 않은 스킬인 경우
            if (!selectedSlot.IsActivated)
                ActivateSkill(selectedSlot); // 스킬 활성화
            // 활성화된 스킬인 경우 즉시 퀵 슬롯 Q에 할당
            else
                quickSlotManager.AssignSkill(_skillSlots[slotID].SkillData, false);
        }
        private void OnPointerEnter(PointerEventData eventData, SkillData skillData, bool isActivated) 
        {
            skillSpecificsPanel.AssignSkill(skillData, eventData, isActivated);
        }
        private void OnPointerExit() 
        {
            skillSpecificsPanel.gameObject.SetActive(false);
        }
        private void OnDragEnd(int slotID, PointerEventData eventData)
        {
            // 퀵 슬롯에 드랍한 경우 퀵슬롯에 스킬 할당
            quickSlotManager.TryDropSlot(eventData)?.Assign(_skillSlots[slotID].SkillData);
        }
        #endregion

        private void OnApplicationQuit()
        {
            // 슬롯들을 가져올 배열
            SkillSlot[] _skillSlots = skillTreeParent.GetComponentsInChildren<SkillSlot>();

            for (int i = 0; i < _skillSlots.Length; i++)
            {
                // 이벤트 해제
                _skillSlots[i].OnMouseLeftClick -= OnSlotLeftClick;
                _skillSlots[i].OnMouseRightClick -= OnSlotRightClick;
                _skillSlots[i].OnMouseEnter -= OnPointerEnter;
                _skillSlots[i].OnMouseExit -= OnPointerExit;
                _skillSlots[i].OnDragEndEvent -= OnDragEnd;
            }
        }
    }
}