using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 기본 슬롯의 기능을 가진 컴포넌트 클래스
     * 작성일: 22년 5월 2일
     */

    public abstract class SlotBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // 슬롯 할당 시 발생 이벤트(슬롯 정보, 아이템 수량)
        public UnityAction <SlotBase, uint> OnAssignEvent;
        // 슬롯 교환(교체) 시 발생 이벤트(현재 슬롯 ID, 이동할 슬롯 ID)
        public UnityAction <int, int> OnSwapEvent;
        // 드래그를 시작할 경우 발생할 이벤트
        public UnityAction<PointerEventData> OnBeginDragEvent;
        // 드래그 종료 시 발생 이벤트(슬롯 ID, 포인터 이벤트 데이터)
        public UnityAction <int, PointerEventData> OnDragEndEvent;
        // 슬롯을 단순 우클릭 했을 시 발생 이벤트(슬롯 ID)
        public UnityAction<int> OnRightClickEvent;
        // 슬롯을 단순 좌클릭 했을 시 발생 이벤트(슬롯 ID)
        public UnityAction<int> OnLeftClickEvent;

        public int slotID { get; private set; } // 슬롯 고유 ID

        [SerializeField] internal bool canDrag; // 슬롯이 드래그 가능한지 여부
        [SerializeField] internal Image iconImage; // 아이콘 이미지

        internal bool IsOnLeftClick; // 단순 좌클릭인지 확인 여부
        internal bool IsOnRightClick; // 단순 우클릭인지 확인 여부
        internal bool IsDragging { get; private set; } // 드래그 중인지 확인

        private bool isAssigned; // 할당된 슬롯인지 여부
        public bool IsAssigned => isAssigned;

        private static GameObject CurrentDraggedObject; // 임시 드래그 아이콘 오브젝트
        private static RectTransform CurrentDraggingTransform; // 임시 드래그 아이콘 트렌스폼
        private static Image CurrentDraggingIconImage; // 임시 드래그 아이콘 이미지

        // 슬롯 아이디 할당
        public void SetSlotID(int id) => slotID = id;

        // 슬롯 할당 함수(이미지 스프라이트)_220504
        public virtual void Assign(Sprite sptire)
        {
            iconImage.gameObject.SetActive(true);

            iconImage.sprite = sptire;
            isAssigned = true;
        }

        // 슬롯 해제하며 아이콘 이미지 제거 및 오브젝트 꺼짐_220503
        public virtual void Unassign()
        {
            isAssigned = false;
            // 아이콘 이미지 제거 및 꺼짐
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }

        // 단순 좌클릭 시 발동 함수_220503
        public virtual void OnLeftClick() { }

        // 단순 우클릭 시 발동 함수_220511
        public virtual void OnRightClick() { }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // 좌 클릭
            if (eventData.button == PointerEventData.InputButton.Left)
                IsOnLeftClick = true;
            // 우 클릭
            else if (eventData.button == PointerEventData.InputButton.Right)
                IsOnRightClick = true;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            // 드래그 없이 단순 좌클릭 시 함수 실행
            if (IsOnLeftClick) OnLeftClick();
            IsOnLeftClick = false;

            // 드래그 없이 단순 우클릭 시 함수 실행
            if (IsOnRightClick) OnRightClick();
            IsOnRightClick = false;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            // 드래그 불가 슬롯인 경우
            if (!canDrag) return;

            IsOnLeftClick = false;
            IsOnRightClick = false;
            IsDragging = true;
            if (!CurrentDraggedObject) CreateTempIcon(); // 임시 드래그 아이콘 생성
            else GetTempIcon(); // 임시 드래그 아이콘 재사용
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            // 드래그 불가 슬롯인 경우
            if (!canDrag) return;

            CurrentDraggingTransform.position = eventData.position;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            // 드래그 불가 슬롯인 경우
            if (!canDrag) return;

            IsDragging = false;
            CurrentDraggedObject.SetActive(false);

            // 드래그 종료 시 이벤트 발생(현재 슬롯 ID, 이벤트 데이터 정보)_220502
            OnDragEndEvent?.Invoke(slotID, eventData);
        }

        // SlotManager를 통해 드랍 이벤트 발생 시 해당 함수 호출됨_220503
        public virtual void SwapSlot(SlotBase targetSlot, bool fixData)
        {
            if (!targetSlot.canDrag || targetSlot == this)
                return;
        }

        // 드래그 시 임시 아이콘 생성_220502
        protected virtual void CreateTempIcon()
        {
            var temp = gameObject.GetComponent<Canvas>();

            if (temp == null)
            {
                Transform t = gameObject.transform.parent;

                while (t != null && temp == null)
                {
                    temp = t.gameObject.GetComponent<Canvas>();
                    t = t.parent;
                }
            }            

            Canvas canvas = temp;

            if (canvas == null || iconImage == null)
                return;

            // Create temporary panel
            CurrentDraggedObject = Instantiate(iconImage.gameObject);
            CurrentDraggedObject.layer = 2; // Ignore Raycast Layer

            CurrentDraggingTransform = CurrentDraggedObject.transform as RectTransform;
            CurrentDraggingTransform.localScale = new Vector3(1f, 1f, 1f);
            CurrentDraggingTransform.SetParent(canvas.transform, false);
            CurrentDraggingTransform.SetAsLastSibling();
            CurrentDraggingTransform.pivot = new Vector2(0.5f, 0.5f);
            CurrentDraggingTransform.anchorMax = Vector2.one * 0.5f;
            CurrentDraggingTransform.anchorMin = Vector2.one * 0.5f;
            CurrentDraggingTransform.sizeDelta = Vector2.one * 70f;

            CurrentDraggingIconImage = CurrentDraggedObject.GetComponent<Image>();
        }

        // 생성된 임시 아이콘 가져오기_220502
        protected virtual void GetTempIcon()
        {
            CurrentDraggedObject.SetActive(true); 
            CurrentDraggingTransform.SetAsLastSibling();
            CurrentDraggingIconImage.sprite = iconImage.sprite;
        }
    }
}
