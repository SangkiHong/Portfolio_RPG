using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.UI;

namespace SK
{
    public class RewardSlot : SlotBase, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text amountText;

        private Item _assignedItem;

        public void Assign(Sprite sptire, uint amount = 1)
        {
            gameObject.SetActive(true);
            iconImage.sprite = sptire;
            _assignedItem = null;

            if (amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = amount.ToString();
            }
            else
                amountText.gameObject.SetActive(false);
        }

        public void Assign(Item item, uint amount = 1)
        {
            gameObject.SetActive(true);
            _assignedItem = item;
            iconImage.sprite = _assignedItem.ItemIcon;

            if (amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = amount.ToString();
            }
            else
                amountText.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_assignedItem != null)
                UIManager.Instance.inventoryManager.itemSpecificsPanel.SetPanel(_assignedItem, transform.position.x);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_assignedItem != null)
                UIManager.Instance.inventoryManager.itemSpecificsPanel.Close();
        }

        public override void Unassign()
        {
            gameObject.SetActive(false);
            amountText.text = string.Empty;
            amountText.gameObject.SetActive(false);
        }
    }
}