using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SK.UI
{
    public class ItemSpecificsPanel : MonoBehaviour
    {
        [SerializeField] private float intervalWithInventoryWindow = 3f;
        
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

        [SerializeField] private Text text_ItemName;
        [SerializeField] private Text text_ItemGrade;
        [SerializeField] private Image text_ItemIcon;

        [Header("Item Status")]
        [SerializeField] private Text text_ItemRequireLevel;

        [SerializeField] private Text text_ItemBaseAbility;
        [SerializeField] private Text text_ItemBaseAbility_Value;

        [SerializeField] private Text text_ItemSubAbility;
        [SerializeField] private Text text_ItemSubAbility_Value;

        [SerializeField] private Text text_ItemWeight;
        [SerializeField] private Text text_ItemWeight_Value;

        [Header("Item Attributes")]
        [SerializeField] private Text text_ItemAttributes;
        [SerializeField] private Text text_ItemDescription;

        [Header("Item Sub Status")]
        [SerializeField] private GameObject itemEffectParent;
        [SerializeField] private Text text_ItemEffectSpecific;
        [SerializeField] private GameObject itemDurabilityParent;
        [SerializeField] private Text text_ItemCurrentDurability;
        [SerializeField] private Text text_ItemMaxDurability;

        private RectTransform _transform;
        private StringBuilder _stringBuilder;
        private Vector3 _panelPos;

        private readonly string _text_AttackPower = "공격력";
        private readonly string _text_Defense = "방어력";
        private readonly string _text_CriticalHit = "치명타";
        private readonly string _text_EvasivePower = "회피력";
        private readonly string _text_DefaultAttributes = "- 수리 가능 장비" + System.Environment.NewLine + "- 장비 강화 가능";

        private float _centerPosX, _panelHalfWidth;

        private void Awake()
        {
            // 초기화
            _transform = transform as RectTransform;
            _stringBuilder = new StringBuilder();

            // 화면 중앙 X 위치
            _centerPosX = Screen.width * 0.5f;
            // 패널 넓이의 절반 크기
            _panelHalfWidth = _transform.sizeDelta.x * 0.5f;
        }

        // 인벤토리매니저로부터 아이템 정보를 받아서 초기화_220508
        public void SetPanel(Item item, float slotPosX)
        {
            gameObject.SetActive(true);

            _stringBuilder.Clear();

            // 아이템 정보 표시
            SetInfo(item);

            // 수정된 위치 값을 저장할 변수 초기화
            _panelPos = Vector3.zero;

            // 슬롯이 좌측에 있는 경우
            if (slotPosX < _centerPosX)
            {
                slotPosX += _panelHalfWidth + intervalWithInventoryWindow;
            }
            // 슬롯이 우측에 있는 경우
            else
            {
                slotPosX -= _panelHalfWidth - intervalWithInventoryWindow;
            }

            // UI 창 좌측 위치 값을 받아서 패널 X값으로 대입
            _panelPos.x = slotPosX;

            // 패널이 화면 아래로 나간 경우 나간 만큼 위쪽으로 이동
            var height = _transform.rect.height;
            var yPos = Input.mousePosition.y;

            if (yPos < height) _panelPos.y = height;
            else _panelPos.y = yPos;

            // 마우스 위치를 기준으로 패널 위치 값 이동
            _transform.position = _panelPos;
        }

        // 아이템 정보 표시_220508
        private void SetInfo(Item item)
        {
            text_ItemName.text = item.ItemName;
            text_ItemGrade.text = item.ItemGrade.ToString();
            text_ItemIcon.sprite = item.ItemIcon;


            text_ItemRequireLevel.text = item.RequiredLevel.ToString();

            // 아이템이 장비류인 경우 기본 능력치 표시
            if (item.ItemType == ItemType.Equipment)
            {
                text_ItemBaseAbility.gameObject.SetActive(true);
                
                // 아이템이 방어구 인 경우
                if (item.EquipmentType != EquipmentType.Weapon)
                {
                    text_ItemBaseAbility.text = _text_Defense;
                    text_ItemSubAbility.text = _text_EvasivePower;
                }
                // 아이템이 무기 인 경우
                else
                {
                    text_ItemBaseAbility.text = _text_AttackPower;
                    text_ItemSubAbility.text = _text_CriticalHit;
                }
                // 기본 능력 수치 표시
                text_ItemBaseAbility_Value.text = item.BaseAbility.ToString();
                // 추가 능력 수치 표시
                text_ItemSubAbility_Value.text = item.SubAbility.ToString();

            }
            else
                text_ItemBaseAbility.gameObject.SetActive(false);

            // 아이템 무게 수치 표시
            text_ItemWeight_Value.text = item.Weight.ToString();

            // 아이템 속성 표시
            text_ItemAttributes.text = _text_DefaultAttributes;
            // 아이템 설명 텍스트 표시
            text_ItemDescription.text = item.Description;

            // 아이템 추가 착용 효과 표시
            if (item.Bonus_Str > 0)
            {
                _stringBuilder.Append("추가 힘 + ");
                _stringBuilder.Append(item.Bonus_Str);
            }
            if (item.Bonus_Dex > 0)
            {
                _stringBuilder.Append("추가 민첩 + ");
                _stringBuilder.Append(item.Bonus_Dex);
            }
            if (item.Bonus_Int > 0)
            {
                _stringBuilder.Append("추가 지능 + ");
                _stringBuilder.Append(item.Bonus_Int);
            }
            
            if (_stringBuilder.Length > 0)
            {
                itemDurabilityParent.SetActive(true);
                text_ItemEffectSpecific.text = _stringBuilder.ToString();
            }
            else
                itemDurabilityParent.SetActive(false);

            // 장비 아이템인 경우 내구도 표시
            if (item.ItemType == ItemType.Equipment)
            {
                itemDurabilityParent.SetActive(true);
                text_ItemCurrentDurability.text = item.Durability.ToString();
                text_ItemMaxDurability.text = item.Durability.ToString();
            }
            else
                itemDurabilityParent.SetActive(false);

            // VerticalLayoutGroup을 통해 UI의 높이를 다시 설정
            verticalLayoutGroup.CalculateLayoutInputVertical();
            verticalLayoutGroup.CalculateLayoutInputHorizontal();
        }
    }
}
