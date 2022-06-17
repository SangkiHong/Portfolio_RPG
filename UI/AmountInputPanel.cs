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

        private readonly string _titleText_Erase = "삭제할 수량 입력";
        private readonly string _titleText_ShopBuy = "구매할 수량 입력";
        private readonly string _titleText_ShopSell = "판매할 수량 입력";
        private readonly string _infoText_Erase = "삭제할 수량";
        private readonly string _infoText_ShopBuy = "구매할 수량";
        private readonly string _infoText_ShopSell = "판매할 수량";

        private int _panelMode; // 0: 인벤토리 아이템 삭제, 1: 상점 구매, 2: 상점 판매
        private uint _maxAmount;
        private uint _inputNumber;

        private void Awake()
        {
            // 초기화
            _transform = transform as RectTransform;

            // 숫자 버튼 이벤트 할당
            for (uint i = 0; i < button_InputAmount.Length; i++)
            {
                uint tempIndex = i; // Closuer problem 으로 인해 임시 인덱스 값을 생성
                // 숫자 입력에 따른 수량 업데이트 함수를 버튼 이벤트에 할당
                button_InputAmount[i].onClick.AddListener(delegate { InputAmountNumber(tempIndex); });
            }

            // 마지막 자릿수를 지우는 버튼 이벤트 할당
            button_EraseLastNum.onClick.AddListener(EraseLastNumber);

            // 입력 값을 리셋하는 버튼 이벤트 할당
            button_Reset.onClick.AddListener(ResetAmount);

            // 선택 수량 확정 버튼 이벤트 할당
            button_Confirm.onClick.AddListener(ConfirmItemAmount);

            // 입력 취소 버튼 이벤트 할당
            button_Cancel.onClick.AddListener(CancelSelectAmount);
        }

        // 최대 갯수를 전달받아 초기화_220507
        public void SetPanel(int panelMode, uint maxAmount)
        {
            // 패널 모드
            _panelMode = panelMode;
            // 입력 수량 초기화
            ResetAmount();
            // 슬롯 아이템의 최대값을 변수에 저장
            _maxAmount = maxAmount;

            // 타이틀, 안내 Text 변경
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

            // 수정된 위치 값을 저장할 변수 초기화
            panelPos = Vector3.zero;

            // 패널이 화면 우측으로 나간 경우 나간 만큼 왼쪽으로 이동
            if (Input.mousePosition.x + _transform.rect.width > Screen.width)
                panelPos.x = Screen.width - (Input.mousePosition.x + _transform.rect.width);
            // 패널이 화면 아래로 나간 경우 나간 만큼 위쪽으로 이동
            if (Input.mousePosition.y < _transform.rect.height)
                panelPos.y = _transform.rect.height - Input.mousePosition.y;            

            // 마우스 위치를 기준으로 패널 위치 값 이동
            _transform.position = Input.mousePosition + panelPos;
        }

        // 숫자 패드를 눌렀을 경우 해당 숫자가 입력되는 이벤트 함수_220507
        private void InputAmountNumber(uint num)
        {
            // 기존 입력된 숫자가 0보다 많으면 10을 곱하여 자릿수를 올려줌
            if (_inputNumber != 0) _inputNumber *= 10;

            // 입력 숫자 값을 더해줌
            _inputNumber += num;

            // 입력 값이 아이템 수량보다 많으면 아이템 최대 값으로 입력
            if (_inputNumber > _maxAmount)
                _inputNumber = _maxAmount;

            // 입력 수량을 텍스트에 표시
            text_ItemAmount.text = _inputNumber.ToString();
        }

        // 1자리 숫자를 지워주는 이벤트 함수_220507
        private void EraseLastNumber()
        {
            // 현재 입력된 수가 10보다 아래면 0이 됨
            if (_inputNumber < 10) 
                _inputNumber = 0;
            else
            {
                float fInputNumber = _inputNumber * 0.1f;
                _inputNumber = (uint)Mathf.Floor(fInputNumber);
            }
            // 입력 수량을 텍스트에 표시
            text_ItemAmount.text = _inputNumber.ToString();
        }

        // 입력된 값을 리셋하는 이벤트 함수_220507
        private void ResetAmount()
        {
            _inputNumber = 0;

            // 입력 수량을 수량 텍스트에 표시
            text_ItemAmount.text = _inputNumber.ToString();
        }

        // 입력 값을 확정하는 이벤트 함수_220507
        private void ConfirmItemAmount()
        {
            OnConfirmAmount?.Invoke(_inputNumber);
            OnConfirmAmount = null;
            gameObject.SetActive(false);
        }

        // 입력 취소하여 패널을 닫고 선택 취소하는 이벤트 함수_220507
        private void CancelSelectAmount()
        {
            OnConfirmAmount?.Invoke(0);
            OnConfirmAmount = null;
            gameObject.SetActive(false);
        }
    }
}
