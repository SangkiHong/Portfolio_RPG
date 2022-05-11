using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SK.UI
{
    public class ItemSpecificsPanel : MonoBehaviour
    {
        [SerializeField] private float intervalWithInventoryWindow = 3f;

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
        [SerializeField] private GameObject text_ItemEffectParent;
        [SerializeField] private Text text_ItemEffectSpecific;
        [SerializeField] private Text text_ItemCurrentDurability;
        [SerializeField] private Text text_ItemMaxDurability;

        private RectTransform _transform;
        private StringBuilder _stringBuilder;
        private Vector3 _panelPos;
        private readonly string text_AttackPower = "���ݷ�";
        private readonly string text_Defense = "����";
        private readonly string text_CriticalHit = "ġ��Ÿ";
        private readonly string text_EvasivePower = "ȸ�Ƿ�";
        private readonly string text_DefaultAttributes = "- ���� ���� ���" + System.Environment.NewLine + "- ��� ��ȭ ����";

        private void Awake()
        {
            // �ʱ�ȭ
            _transform = transform as RectTransform;
            _stringBuilder = new StringBuilder();
        }

        // �κ��丮�Ŵ����κ��� ������ ������ �޾Ƽ� �ʱ�ȭ_220508
        public void SetPanel(Item item, float inventoryRectPosMinX)
        {
            gameObject.SetActive(true);

            _stringBuilder.Clear();

            // ������ ���� ǥ��
            SetInfo(item);

            // ������ ��ġ ���� ������ ���� �ʱ�ȭ
            _panelPos = Vector3.zero;

            // �κ��丮 â ���� ��ġ ���� �޾Ƽ� �г� X������ ����
            _panelPos.x = inventoryRectPosMinX - intervalWithInventoryWindow;

            // �г��� ȭ�� �Ʒ��� ���� ��� ���� ��ŭ �������� �̵�
            if (Input.mousePosition.y < _transform.rect.height)
                _panelPos.y = _transform.rect.height;
            else
                _panelPos.y = Input.mousePosition.y;

            // ���콺 ��ġ�� �������� �г� ��ġ �� �̵�
            _transform.position = _panelPos;
        }

        // ������ ���� ǥ��_220508
        private void SetInfo(Item item)
        {
            text_ItemName.text = item.itemName;
            text_ItemGrade.text = item.itemGrade.ToString();
            text_ItemIcon.sprite = item.itemIcon;


            text_ItemRequireLevel.text = item.requiredLevel.ToString();

            // �������� ������ ��� �⺻ �ɷ�ġ ǥ��
            if (item.itemType == ItemType.Equipment)
            {
                text_ItemBaseAbility.gameObject.SetActive(true);
                
                // �������� �� �� ���
                if (item.equipmentType != EquipmentType.Weapon)
                {
                    text_ItemBaseAbility.text = text_Defense;
                    text_ItemSubAbility.text = text_EvasivePower;
                }
                // �������� ���� �� ���
                else
                {
                    text_ItemBaseAbility.text = text_AttackPower;
                    text_ItemSubAbility.text = text_CriticalHit;
                }
                // �⺻ �ɷ� ��ġ ǥ��
                text_ItemBaseAbility_Value.text = item.baseAbility.ToString();
                // �߰� �ɷ� ��ġ ǥ��
                text_ItemSubAbility_Value.text = item.subAbility.ToString();

            }
            else
                text_ItemBaseAbility.gameObject.SetActive(false);

            // ������ ���� ��ġ ǥ��
            text_ItemWeight_Value.text = item.weight.ToString();

            // ������ �Ӽ� ǥ��
            text_ItemAttributes.text = text_DefaultAttributes;
            // ������ ���� �ؽ�Ʈ ǥ��
            text_ItemDescription.text = item.description;

            // ������ �߰� ���� ȿ�� ǥ��
            if (item.bonus_Str > 0)
            {
                _stringBuilder.Append("�߰� �� + ");
                _stringBuilder.Append(item.bonus_Str);
            }
            if (item.bonus_Dex > 0)
            {
                _stringBuilder.Append("�߰� ��ø + ");
                _stringBuilder.Append(item.bonus_Dex);
            }
            if (item.bonus_Int > 0)
            {
                _stringBuilder.Append("�߰� ���� + ");
                _stringBuilder.Append(item.bonus_Int);
            }
            
            if (_stringBuilder.Length > 0)
            {
                text_ItemEffectParent.SetActive(true);
                text_ItemEffectSpecific.text = _stringBuilder.ToString();
            }
            else
                text_ItemEffectParent.SetActive(false);

            // �ش� ������ ������ ǥ��
            text_ItemCurrentDurability.text = item.durability.ToString();
            text_ItemMaxDurability.text = item.durability.ToString();

        }
    }
}
