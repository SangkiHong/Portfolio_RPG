using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class CurrencySyncHandler : MonoBehaviour
    {
        [SerializeField] private Text[] goldTexts;
        [SerializeField] private Text[] gemTexts;

        private Data.PlayerData _playerData;

        uint lastGold, lastGem;

        public void Initialize()
        {
            _playerData = Data.DataManager.Instance.PlayerData;
            GameManager.Instance.DataManager.OnChangedCurrency += Sync;
            Sync();
        }

        public void Sync()
        {
            // 골드 업데이트
            if (lastGold != _playerData.Gold)
            {
                // 최근 골드 업데이트
                lastGold = _playerData.Gold;
                // 골드 표시 Text UI 업데이트
                for (int i = 0; i < goldTexts.Length; i++)
                    goldTexts[i].text = lastGold.ToString();
            }

            // 잼 업데이트
            if (lastGem != _playerData.Gem)
            {
                // 최근 골드 업데이트
                lastGem = _playerData.Gem;
                // 골드 표시 Text UI 업데이트
                for (int i = 0; i < gemTexts.Length; i++)
                    gemTexts[i].text = lastGem.ToString();
            }
        }
    }
}
