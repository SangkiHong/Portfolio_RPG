using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class AmountInputPanel : MonoBehaviour
    {
        public delegate void ConfirmHandler(uint amount);

        [Header("Buttons")]
        [SerializeField] private Button[] button_InputAmount;
        [SerializeField] private Button button_EraseLastNum;
        [SerializeField] private Button button_Reset;
        [SerializeField] private Button button_Cancel;
        [SerializeField] private Button button_Confirm;

        [Header("Text")]
        [SerializeField] private Text text_ItemAmount;
        [SerializeField] private Text text_Title;
        [SerializeField] private Text text_Info;

        public event ConfirmHandler OnConfirmAmount;

        private RectTransform _transform;
        private Vector3 panelPos;

        private readonly string _titleText_Erase = "������ ���� �Է�";
        private readonly string _titleText_ShopBuy = "������ ���� �Է�";
        private readonly string _titleText_ShopSell = "�Ǹ��� ���� �Է�";
        private readonly string _infoText_Erase = "������ ����";
        private readonly string _infoText_ShopBuy = "������ ����";
        private readonly string _infoText_ShopSell = "�Ǹ��� ����";

        private int _panelMode; // 0: �κ��丮 ������ ����, 1: ���� ����, 2: ���� �Ǹ�
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
                // ���� �Է¿� ���� ���� ������Ʈ �Լ��� ��ư �̺�Ʈ�� �Ҵ�
                button_InputAmount[i].onClick.AddListener(delegate { InputAmountNumber(tempIndex); });
            }

            // ������ �ڸ����� ����� ��ư �̺�Ʈ �Ҵ�
            button_EraseLastNum.onClick.AddListener(EraseLastNumber);

            // �Է� ���� �����ϴ� ��ư �̺�Ʈ �Ҵ�
            button_Reset.onClick.AddListener(ResetAmount);

            // ���� ���� Ȯ�� ��ư �̺�Ʈ �Ҵ�
            button_Confirm.onClick.AddListener(ConfirmItemAmount);

            // �Է� ��� ��ư �̺�Ʈ �Ҵ�
            button_Cancel.onClick.AddListener(CancelSelectAmount);
        }

        // �ִ� ������ ���޹޾� �ʱ�ȭ_220507
        public void SetPanel(int panelMode, uint maxAmount)
        {
            // �г� ���
            _panelMode = panelMode;
            // �Է� ���� �ʱ�ȭ
            ResetAmount();
            // ���� �������� �ִ밪�� ������ ����
            _maxAmount = maxAmount;

            // Ÿ��Ʋ, �ȳ� Text ����
            switch (_panelMode)
            {
                case 0:
                    text_Title.text = _titleText_Erase;
                    text_Info.text = _infoText_Erase;
                    break;
                case 1:
                    text_Title.text = _titleText_ShopBuy;
                    text_Info.text = _infoText_ShopBuy;
                    break;
                case 2:
                    text_Title.text = _titleText_ShopSell;
                    text_Info.text = _infoText_ShopSell;
                    break;
            }

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

            // �Է� ������ �ؽ�Ʈ�� ǥ��
            text_ItemAmount.text = _inputNumber.ToString();
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
            // �Է� ������ �ؽ�Ʈ�� ǥ��
            text_ItemAmount.text = _inputNumber.ToString();
        }

        // �Էµ� ���� �����ϴ� �̺�Ʈ �Լ�_220507
        private void ResetAmount()
        {
            _inputNumber = 0;

            // �Է� ������ ���� �ؽ�Ʈ�� ǥ��
            text_ItemAmount.text = _inputNumber.ToString();
        }

        // �Է� ���� Ȯ���ϴ� �̺�Ʈ �Լ�_220507
        private void ConfirmItemAmount()
        {
            OnConfirmAmount?.Invoke(_inputNumber);
            OnConfirmAmount = null;
            gameObject.SetActive(false);
        }

        // �Է� ����Ͽ� �г��� �ݰ� ���� ����ϴ� �̺�Ʈ �Լ�_220507
        private void CancelSelectAmount()
        {
            OnConfirmAmount?.Invoke(0);
            OnConfirmAmount = null;
            gameObject.SetActive(false);
        }
    }
}
