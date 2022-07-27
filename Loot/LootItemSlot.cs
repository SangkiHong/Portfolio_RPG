using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.Loot
{
    /* 작성자: 홍상기
     * 내용: 전리품(드랍) 아이템 슬롯
     * 작성일: 22년 7월 20일
     */

    public class LootItemSlot : MonoBehaviour, IPointerClickHandler
    {
        public delegate void SelectHandler(int instanceID);
        public delegate void JustLootingHandler(int instanceID);
        public event SelectHandler onSelect;
        public event JustLootingHandler onLooting;

        [SerializeField] private Image image_SlotBase;
        [SerializeField] private Image image_Icon;
        [SerializeField] private Text text_ItemName;
        [SerializeField] private Text text_ItemAmount;

        [SerializeField] private Color defaultBaseColor;

        private Item _assignedItem;
        public Item AssignedItem => _assignedItem;

        private int _instanceID;

        private void Awake()
            => _instanceID = GetInstanceID();

        // 슬롯 할당
        public void Assign(Item item, int amount)
        {
            // 초기화::할당된 아이템 데이터를 변수에 저장
            _assignedItem = item;

            // 초기화::슬롯 베이스 색상(선택 안된 상태)
            SelectControl(false);
            // 아이템 텍스트 정보 할당
            image_Icon.sprite = item.ItemIcon;
            text_ItemName.text = item.ItemName;
            
            // 초기화::아이템 수량이 2 이상인 경우 수량 텍스트 UI 표시
            if (amount >= 2)
            {
                text_ItemAmount.gameObject.SetActive(true);
                text_ItemAmount.text = amount.ToString();
            }
            else
                text_ItemAmount.gameObject.SetActive(false);

            // 슬롯 오브젝트 켜기
            gameObject.SetActive(true);
        }

        // 슬롯 할당 해제
        public void UnAssign()
        {
            SelectControl(false);
            _assignedItem = null;
            image_Icon.sprite = null;
            text_ItemName.text = string.Empty;
            onSelect = null;
            onLooting = null;

            // 슬롯 오브젝트 꺼짐
            gameObject.SetActive(false);
        }

        public void SelectControl(bool isSelected)
        {
            // 선택된 상태
            if (isSelected)
            {
                defaultBaseColor.a = 1;
            }
            else
            {
                defaultBaseColor.a = 0;
            }

            // 슬롯 베이스 색상 표시
            image_SlotBase.color = defaultBaseColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 좌클릭한 경우 슬롯 선택
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                SelectControl(true);
                onSelect?.Invoke(_instanceID);
            }
            // 우클릭한 경우 즉시 루팅 실행
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                onLooting?.Invoke(_instanceID);
            }
        }
    }
}