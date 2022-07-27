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
            // ��� ������Ʈ
            if (lastGold != _playerData.Gold)
            {
                // �ֱ� ��� ������Ʈ
                lastGold = _playerData.Gold;
                // ��� ǥ�� Text UI ������Ʈ
                for (int i = 0; i < goldTexts.Length; i++)
                    goldTexts[i].text = lastGold.ToString();
            }

            // �� ������Ʈ
            if (lastGem != _playerData.Gem)
            {
                // �ֱ� ��� ������Ʈ
                lastGem = _playerData.Gem;
                // ��� ǥ�� Text UI ������Ʈ
                for (int i = 0; i < gemTexts.Length; i++)
                    gemTexts[i].text = lastGem.ToString();
            }
        }
    }
}
