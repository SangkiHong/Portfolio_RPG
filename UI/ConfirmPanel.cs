using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ��Ȳ�� ���� Ȯ�� ���θ� ����ڷκ��� ������� UI �г� ������Ʈ
     * �ۼ���: 22�� 6�� 17��
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

        // ���� �г��� �������� �ִ� ���� ���� ���θ� ��ȯ
        public bool IsShow { get; private set; }

        private void Initialize()
        {
            _init = true;

            // Ȯ�� ��ư �̺�Ʈ �Լ� �Ҵ�
            btn_Confirm.onClick.AddListener(delegate { Confirm(); });
            btn_Cancel.onClick.AddListener(delegate { Cancel(); });

            // ��ǲ �̺�Ʈ �Լ� �Ҵ�
            _inputConfirm = GameManager.Instance.InputManager.playerInput.actions["Confirm"];
            _inputConfirm.started += InputConfirm;
        }

        // �ȳ� Ÿ�Կ� ���� UI ǥ�ø� ��Ÿ�� �Լ�
        public void ShowInfo(InfoType infoType)
        {
            if (!_init)
                Initialize();

            IsShow = true;

            // �г� UI ǥ��
            gameObject.SetActive(true);
            // Ÿ�Կ� ���� �ȳ� UI �Ҵ�
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

            // ��ǲ �׼� Ȱ��ȭ
            _inputConfirm.Enable();
            transform.SetAsLastSibling();
        }

        // Ȯ�� ��ư ���� �� ȣ��� �Լ�
        private void Confirm()
        {
            IsShow = false;
            // Ȯ�� ��ư �̺�Ʈ ȣ��
            OnConfirmed?.Invoke();
            // �̺�Ʈ �ʱ�ȭ
            OnConfirmed = null;
            // �г� ����
            gameObject.SetActive(false);
            // ��ǲ �׼� ��Ȱ��ȭ
            _inputConfirm.Disable();
        }

        // ��� ��ư ���� �� ȣ��� �Լ�
        public void Cancel()
        {
            IsShow = false;
            // �̺�Ʈ �ʱ�ȭ
            OnConfirmed = null;
            // �г� ����
            gameObject.SetActive(false);
            // ��ǲ �׼� ��Ȱ��ȭ
            _inputConfirm.Disable();
        }

        // ��� ��ư�� ��Ÿ�� ���� ���� �Լ�
        private void SwitchCancelButton(bool show)
        {
            if (show && !btn_Cancel.gameObject.activeSelf)
                btn_Cancel.gameObject.SetActive(true);
            else if (!show && btn_Cancel.gameObject.activeSelf) 
                btn_Cancel.gameObject.SetActive(false);
        }

        // R ��ư�� ���� �� Ȯ�� ��ư �۵�
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
