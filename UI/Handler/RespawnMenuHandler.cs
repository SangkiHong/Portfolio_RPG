using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class RespawnMenuHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Text text_DeathInfo;
        [SerializeField] private Button button_ImmediatelyRespawn;
        [SerializeField] private Button button_VillageRespawn;

        private readonly string _replaceTarget = "***";
        private string _defualtInfoText;

        private void Start()
        {
            _defualtInfoText = text_DeathInfo.text;

            button_ImmediatelyRespawn.onClick.AddListener(ImmediatelyRespawn);
            button_VillageRespawn.onClick.AddListener(VillageRespawn);
        }

        public void Show(string deathReason)
        {
            text_DeathInfo.text = text_DeathInfo.text.Replace(_replaceTarget, deathReason);
            SceneManager.Instance.OnUpdate += ShowMenu;
        }

        private void ShowMenu()
        {
            menuCanvasGroup.alpha += Time.deltaTime;
            if (menuCanvasGroup.alpha >= 1)
            {
                menuCanvasGroup.blocksRaycasts = true;
                SceneManager.Instance.OnUpdate -= ShowMenu;
            }
        }

        private void Hide()
        {
            menuCanvasGroup.blocksRaycasts = false;
            menuCanvasGroup.alpha = 0;
            text_DeathInfo.text = _defualtInfoText;
        }

        // 즉시 리스폰 버튼 이벤트 함수
        private void ImmediatelyRespawn()
        {
            Hide();
            GameManager.Instance.Respawn(true);
        }

        // 가까운 마을에서 리스폰 버튼 이벤트 함수
        private void VillageRespawn()
        {
            Hide();
            GameManager.Instance.Respawn();
        }
    }
}