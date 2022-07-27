using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 상황에 따른 확인 여부를 사용자로부터 응답받을 UI 패널 컴포넌트
     * 작성일: 22년 6월 17일
     */

    public enum InfoType : int
    {
        DeleteItem,
        SellItem,
        UnequipItem,
        NotEnoughCurruncy,
        NotEnoughSlot
    }

    public class ConfirmPanel : MonoBehaviour
    {
        public delegate void ConfirmCallbackHandler();

        [Header("Text")]
        [SerializeField] private Text text_Title;
        [SerializeField] private Text text_Info;

        [Header("Button")]
        [SerializeField] private Button btn_Confirm;
        [SerializeField] private Button btn_Cancel;

        public event ConfirmCallbackHandler OnConfirmed;

        private InputAction _inputConfirm;

        private bool _init;

        // 현재 패널이 보여지고 있는 지에 대한 여부를 반환
        public bool IsShow { get; private set; }

        private void Initialize()
        {
            _init = true;

            // 확인 버튼 이벤트 함수 할당
            btn_Confirm.onClick.AddListener(delegate { Confirm(); });
            btn_Cancel.onClick.AddListener(delegate { Cancel(); });

            // 인풋 이벤트 함수 할당
            _inputConfirm = GameManager.Instance.InputManager.playerInput.actions["Confirm"];
            _inputConfirm.started += InputConfirm;
        }

        // 안내 타입에 따라 UI 표시를 나타낼 함수
        public void ShowInfo(InfoType infoType)
        {
            if (!_init)
                Initialize();

            IsShow = true;

            // 패널 UI 표시
            gameObject.SetActive(true);
            // 타입에 따른 안내 UI 할당
            switch (infoType)
            {
                case InfoType.DeleteItem:
                    text_Title.text = Strings.Info_DeleteItem_TItle;
                    text_Info.text = Strings.Info_DeleteItem_Info;
                    SwitchCancelButton(true);
                    break;
                case InfoType.SellItem:
                    text_Title.text = Strings.Info_SellItem_Title;
                    text_Info.text = Strings.Info_SellItem_Info;
                    SwitchCancelButton(true);
                    break;
                case InfoType.UnequipItem:
                    text_Title.text = Strings.Info_UnequipItem_Title;
                    text_Info.text = Strings.Info_UnequipItem_Info;
                    SwitchCancelButton(false);
                    break;
                case InfoType.NotEnoughCurruncy:
                    text_Title.text = Strings.Info_NotEnoughCurruncy_Title;
                    text_Info.text = Strings.Info_NotEnoughCurruncy_Info;
                    SwitchCancelButton(false);
                    break;
                case InfoType.NotEnoughSlot:
                    text_Title.text = Strings.Info_NotEnoughSlot_Title;
                    text_Info.text = Strings.Info_NotEnoughSlot_Info;
                    SwitchCancelButton(false);
                    break;
            }

            // 인풋 액션 활성화
            _inputConfirm.Enable();
            transform.SetAsLastSibling();
        }

        // 확인 버튼 누를 시 호출될 함수
        private void Confirm()
        {
            IsShow = false;
            // 확인 버튼 이벤트 호출
            OnConfirmed?.Invoke();
            // 이벤트 초기화
            OnConfirmed = null;
            // 패널 닫음
            gameObject.SetActive(false);
            // 인풋 액션 비활성화
            _inputConfirm.Disable();
        }

        // 취소 버튼 누를 시 호출될 함수
        public void Cancel()
        {
            IsShow = false;
            // 이벤트 초기화
            OnConfirmed = null;
            // 패널 닫음
            gameObject.SetActive(false);
            // 인풋 액션 비활성화
            _inputConfirm.Disable();
        }

        // 취소 버튼을 나타낼 지에 대한 함수
        private void SwitchCancelButton(bool show)
        {
            if (show && !btn_Cancel.gameObject.activeSelf)
                btn_Cancel.gameObject.SetActive(true);
            else if (!show && btn_Cancel.gameObject.activeSelf) 
                btn_Cancel.gameObject.SetActive(false);
        }

        // R 버튼을 누를 시 확인 버튼 작동
        private void InputConfirm(InputAction.CallbackContext context)
        {
            Confirm();
        }

        private void OnApplicationQuit()
        {
            _inputConfirm.started -= InputConfirm;
        }
    }
}
