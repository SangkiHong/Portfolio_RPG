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
            // 초기화
            _transform = transform as RectTransform;

            // 숫자 버튼 이벤트 할당
            for (uint i = 0; i < button_InputAmount.Length; i++)
            {
                uint tempIndex = i; // Closuer problem 으로 인해 임시 인덱스 값을 생성
                // 숫자 입력에 따른 삭제할 수량 업데이트 함수릴 버튼 이벤트로 할당
                button_InputAmount[i].onClick.AddListener(delegate { InputAmountNumber(tempIndex); });
            }

            // 마지막 자릿수를 지우는 버튼 이벤트 할당
            button_EraseLastNum.onClick.AddListener(EraseLastNumber);

            // 입력 값을 리셋하는 버튼 이벤트 할당
            button_Reset.onClick.AddListener(ResetAmount);

            // 입력에 따른 아이템 삭제하는 버튼 이벤트 할당
            button_Confirm.onClick.AddListener(ConfirmEraseItem);

            // 입력 취소 버튼 이벤트 할당
            button_Cancel.onClick.AddListener(CancelSelectAmount);
        }

        // 인벤토리매니저로부터 슬롯 정보를 받아서 초기화_220507
        public void SetPaenl(uint maxAmount)
        {
            // 입력 수량 초기화
            ResetAmount();
            // 슬롯 아이템의 최대값을 변수에 저장
            _maxAmount = maxAmount;
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

            // 삭제 수량 텍스트에 표시
            text_eraseAmount.text = _inputNumber.ToString();
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
            // 삭제 수량 텍스트에 표시
            text_eraseAmount.text = _inputNumber.ToString();
        }

        // 입력된 값을 리셋하는 이벤트 함수_220507
        private void ResetAmount()
        {
            _inputNumber = 0;

            // 삭제 수량 텍스트에 표시
            text_eraseAmount.text = _inputNumber.ToString();
        }

        // 입력 값에 따른 아이템 삭제하는 이벤트 함수_220507
        private void ConfirmEraseItem()
        {
            inventoryManager.ConfirmEraseAmount(_inputNumber);
            gameObject.SetActive(false);
        }

        // 입력 취소하여 패널을 닫고 선택 취소하는 이벤트 함수_220507
        private void CancelSelectAmount()
        {
            inventoryManager.ConfirmEraseAmount(0);
            gameObject.SetActive(false);
        }
    }
}
