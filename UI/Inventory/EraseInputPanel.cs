using UnityEngine;
using UnityEngine.UI;

namespace SK
{
    public class EraseInputPanel : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UI.InventoryManager inventoryManager;

        [Header("Buttons")]
        [SerializeField] private Button[] button_InputAmount;
        [SerializeField] private Button button_EraseLastNum;
        [SerializeField] private Button button_Reset;
        [SerializeField] private Button button_Cancel;
        [SerializeField] private Button button_Confirm;

        [Header("Buttons")]
        [SerializeField] private Text text_eraseAmount;

        private RectTransform _transform;
        private Vector3 panelPos;
        private uint _maxAmount;
        private uint _inputNumber;

        private void Awake()
        {
            // �ʱ�ȭ
            _transform = transform as RectTransform;

            // ���� ��ư �̺�Ʈ �Ҵ�
            for (uint i = 0; i < button_InputAmount.Length; i++)
            {
                uint tempIndex = i; // Closuer problem ���� ���� �ӽ� �ε��� ���� ����
                // ���� �Է¿� ���� ������ ���� ������Ʈ �Լ��� ��ư �̺�Ʈ�� �Ҵ�
                button_InputAmount[i].onClick.AddListener(delegate { InputAmountNumber(tempIndex); });
            }

            // ������ �ڸ����� ����� ��ư �̺�Ʈ �Ҵ�
            button_EraseLastNum.onClick.AddListener(EraseLastNumber);

            // �Է� ���� �����ϴ� ��ư �̺�Ʈ �Ҵ�
            button_Reset.onClick.AddListener(ResetAmount);

            // �Է¿� ���� ������ �����ϴ� ��ư �̺�Ʈ �Ҵ�
            button_Confirm.onClick.AddListener(ConfirmEraseItem);

            // �Է� ��� ��ư �̺�Ʈ �Ҵ�
            button_Cancel.onClick.AddListener(CancelSelectAmount);
        }

        // �κ��丮�Ŵ����κ��� ���� ������ �޾Ƽ� �ʱ�ȭ_220507
        public void SetPaenl(uint maxAmount)
        {
            // �Է� ���� �ʱ�ȭ
            ResetAmount();
            // ���� �������� �ִ밪�� ������ ����
            _maxAmount = maxAmount;
            gameObject.SetActive(true);

            // ������ ��ġ ���� ������ ���� �ʱ�ȭ
            panelPos = Vector3.zero;

            // �г��� ȭ�� �������� ���� ��� ���� ��ŭ �������� �̵�
            if (Input.mousePosition.x + _transform.rect.width > Screen.width)
                panelPos.x = Screen.width - (Input.mousePosition.x + _transform.rect.width);
            // �г��� ȭ�� �Ʒ��� ���� ��� ���� ��ŭ �������� �̵�
            if (Input.mousePosition.y < _transform.rect.height)
                panelPos.y = _transform.rect.height - Input.mousePosition.y;            

            // ���콺 ��ġ�� �������� �г� ��ġ �� �̵�
            _transform.position = Input.mousePosition + panelPos;
        }

        // ���� �е带 ������ ��� �ش� ���ڰ� �ԷµǴ� �̺�Ʈ �Լ�_220507
        private void InputAmountNumber(uint num)
        {
            // ���� �Էµ� ���ڰ� 0���� ������ 10�� ���Ͽ� �ڸ����� �÷���
            if (_inputNumber != 0) _inputNumber *= 10;

            // �Է� ���� ���� ������
            _inputNumber += num;

            // �Է� ���� ������ �������� ������ ������ �ִ� ������ �Է�
            if (_inputNumber > _maxAmount)
                _inputNumber = _maxAmount;

            // ���� ���� �ؽ�Ʈ�� ǥ��
            text_eraseAmount.text = _inputNumber.ToString();
        }

        // 1�ڸ� ���ڸ� �����ִ� �̺�Ʈ �Լ�_220507
        private void EraseLastNumber()
        {
            // ���� �Էµ� ���� 10���� �Ʒ��� 0�� ��
            if (_inputNumber < 10) 
                _inputNumber = 0;
            else
            {
                float fInputNumber = _inputNumber * 0.1f;
                _inputNumber = (uint)Mathf.Floor(fInputNumber);
            }
            // ���� ���� �ؽ�Ʈ�� ǥ��
            text_eraseAmount.text = _inputNumber.ToString();
        }

        // �Էµ� ���� �����ϴ� �̺�Ʈ �Լ�_220507
        private void ResetAmount()
        {
            _inputNumber = 0;

            // ���� ���� �ؽ�Ʈ�� ǥ��
            text_eraseAmount.text = _inputNumber.ToString();
        }

        // �Է� ���� ���� ������ �����ϴ� �̺�Ʈ �Լ�_220507
        private void ConfirmEraseItem()
        {
            inventoryManager.ConfirmEraseAmount(_inputNumber);
            gameObject.SetActive(false);
        }

        // �Է� ����Ͽ� �г��� �ݰ� ���� ����ϴ� �̺�Ʈ �Լ�_220507
        private void CancelSelectAmount()
        {
            inventoryManager.ConfirmEraseAmount(0);
            gameObject.SetActive(false);
        }
    }
}
