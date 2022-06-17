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

        private readonly string _text_AttackPower = "���ݷ�";
        private readonly string _text_Defense = "����";
        private readonly string _text_CriticalHit = "ġ��Ÿ";
        private readonly string _text_EvasivePower = "ȸ�Ƿ�";
        private readonly string _text_DefaultAttributes = "- ���� ���� ���" + System.Environment.NewLine + "- ��� ��ȭ ����";

        private float _centerPosX, _panelHalfWidth;

        private void Awake()
        {
            // �ʱ�ȭ
            _transform = transform as RectTransform;
            _stringBuilder = new StringBuilder();

            // ȭ�� �߾� X ��ġ
            _centerPosX = Screen.width * 0.5f;
            // �г� ������ ���� ũ��
            _panelHalfWidth = _transform.sizeDelta.x * 0.5f;
        }

        // �κ��丮�Ŵ����κ��� ������ ������ �޾Ƽ� �ʱ�ȭ_220508
        public void SetPanel(Item item, float slotPosX)
        {
            gameObject.SetActive(true);

            _stringBuilder.Clear();

            // ������ ���� ǥ��
            SetInfo(item);

            // ������ ��ġ ���� ������ ���� �ʱ�ȭ
            _panelPos = Vector3.zero;

            // ������ ������ �ִ� ���
            if (slotPosX < _centerPosX)
            {
                slotPosX += _panelHalfWidth + intervalWithInventoryWindow;
            }
            // ������ ������ �ִ� ���
            else
            {
                slotPosX -= _panelHalfWidth - intervalWithInventoryWindow;
            }

            // UI â ���� ��ġ ���� �޾Ƽ� �г� X������ ����
            _panelPos.x = slotPosX;

            // �г��� ȭ�� �Ʒ��� ���� ��� ���� ��ŭ �������� �̵�
            var height = _transform.rect.height;
            var yPos = Input.mousePosition.y;

            if (yPos < height) _panelPos.y = height;
            else _panelPos.y = yPos;

            // ���콺 ��ġ�� �������� �г� ��ġ �� �̵�
            _transform.position = _panelPos;
        }

        // ������ ���� ǥ��_220508
        private void SetInfo(Item item)
        {
            text_ItemName.text = item.ItemName;
            text_ItemGrade.text = item.ItemGrade.ToString();
            text_ItemIcon.sprite = item.ItemIcon;


            text_ItemRequireLevel.text = item.RequiredLevel.ToString();

            // �������� ������ ��� �⺻ �ɷ�ġ ǥ��
            if (item.ItemType == ItemType.Equipment)
            {
                text_ItemBaseAbility.gameObject.SetActive(true);
                
                // �������� �� �� ���
                if (item.EquipmentType != EquipmentType.Weapon)
                {
                    text_ItemBaseAbility.text = _text_Defense;
                    text_ItemSubAbility.text = _text_EvasivePower;
                }
                // �������� ���� �� ���
                else
                {
                    text_ItemBaseAbility.text = _text_AttackPower;
                    text_ItemSubAbility.text = _text_CriticalHit;
                }
                // �⺻ �ɷ� ��ġ ǥ��
                text_ItemBaseAbility_Value.text = item.BaseAbility.ToString();
                // �߰� �ɷ� ��ġ ǥ��
                text_ItemSubAbility_Value.text = item.SubAbility.ToString();

            }
            else
                text_ItemBaseAbility.gameObject.SetActive(false);

            // ������ ���� ��ġ ǥ��
            text_ItemWeight_Value.text = item.Weight.ToString();

            // ������ �Ӽ� ǥ��
            text_ItemAttributes.text = _text_DefaultAttributes;
            // ������ ���� �ؽ�Ʈ ǥ��
            text_ItemDescription.text = item.Description;

            // ������ �߰� ���� ȿ�� ǥ��
            if (item.Bonus_Str > 0)
            {
                _stringBuilder.Append("�߰� �� + ");
                _stringBuilder.Append(item.Bonus_Str);
            }
            if (item.Bonus_Dex > 0)
            {
                _stringBuilder.Append("�߰� ��ø + ");
                _stringBuilder.Append(item.Bonus_Dex);
            }
            if (item.Bonus_Int > 0)
            {
                _stringBuilder.Append("�߰� ���� + ");
                _stringBuilder.Append(item.Bonus_Int);
            }
            
            if (_stringBuilder.Length > 0)
            {
                itemDurabilityParent.SetActive(true);
                text_ItemEffectSpecific.text = _stringBuilder.ToString();
            }
            else
                itemDurabilityParent.SetActive(false);

            // ��� �������� ��� ������ ǥ��
            if (item.ItemType == ItemType.Equipment)
            {
                itemDurabilityParent.SetActive(true);
                text_ItemCurrentDurability.text = item.Durability.ToString();
                text_ItemMaxDurability.text = item.Durability.ToString();
            }
            else
                itemDurabilityParent.SetActive(false);

            // VerticalLayoutGroup�� ���� UI�� ���̸� �ٽ� ����
            verticalLayoutGroup.CalculateLayoutInputVertical();
            verticalLayoutGroup.CalculateLayoutInputHorizontal();
        }
    }
}
