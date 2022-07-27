using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 스킬 트리에 표시될 스킬 정보를 담은 슬롯 컴포넌트
     * 작성일: 22년 6월 19일
     */

    public class SkillSlot : SlotBase, IPointerEnterHandler, IPointerExitHandler
    {
        // 마우스 상황에 따른 발생 이벤트
        public UnityAction<int> OnMouseLeftClick;
        public UnityAction<int> OnMouseRightClick;
        public UnityAction<PointerEventData, Data.SkillData, bool> OnMouseEnter;
        public UnityAction OnMouseExit;

        // 슬롯에 할당된 스킬 데이터
        [SerializeField] private Data.SkillData skillData;
        // 스킬 이름
        [SerializeField] private Text text_SkillName;
        // 스킬 간단 설명
        [SerializeField] private Text text_SkillDescription;
        // 비활성된 스킬을 어둡게 표시해 줄 오버레이 오브젝트
        [SerializeField] private GameObject blackOverlay;
        // 쿨타임 표시할 슬라이더
        [SerializeField] private Slider coolTimeSlider;

        private float _skillCoolTime;

        // 프로퍼티
        public Data.SkillData SkillData => skillData;
        public bool IsActivated { get; private set; }

        private void Awake()
            => Assign(skillData);

        // 슬롯에 스킬 정보를 할당
        private void Assign(Data.SkillData skillData)
        {
            iconImage.sprite = skillData.skillIcon;
            text_SkillName.text = skillData.skillName;
            text_SkillDescription.text = skillData.skillDescription;
            _skillCoolTime = skillData.skillCoolTime;
            blackOverlay.SetActive(true);
            IsActivated = false;
        }

        // 스킬 활성화
        public void Active()
        {
            // 스킬 활성화
            IsActivated = true;
            // 드래그 앤 드랍으로 슬롯에 장착 가능
            canDrag = true;
            // 오버레이 끔
            blackOverlay.SetActive(false);
        }

        // 스킬 쿨타임 표시
        public void UpdateCoolTime(float remainCoolTime)
        {
            var value = remainCoolTime / _skillCoolTime;
            if (value > 0)
            {
                if (!coolTimeSlider.gameObject.activeSelf)
                    coolTimeSlider.gameObject.SetActive(true);
                coolTimeSlider.value = value;
            }
            else
                coolTimeSlider.gameObject.SetActive(false);
        }

        #region Event
        // 마우스 좌클릭한 경우 호출될 이벤트 함수
        public override void OnLeftClick()
        {
            OnMouseLeftClick?.Invoke(slotID);
        }

        // 마우스 좌클릭한 경우 호출될 이벤트 함수
        public override void OnRightClick()
        {
            OnMouseRightClick?.Invoke(slotID);
        }

        // 마우스를 슬롯에 올릴 경우 호출될 이벤트 함수
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke(eventData, skillData, IsActivated);
        }

        // 마우스가 슬롯에서 빠져나갈 경우 호출될 이벤트 함수
        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit?.Invoke();
        }
        #endregion
    }
}