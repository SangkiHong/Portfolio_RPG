using System.Collections.Generic;
using UnityEngine;
using SK.Quests;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: �÷��� ȭ�鿡 ǥ�õ� ����Ʈ �̴� ����â ������ Ŭ����
     * �ۼ���: 22�� 6�� 16��
     */

    public class QuestMiniInfoManager : MonoBehaviour
    {        
        [Header("Prefab")]
        [SerializeField] private GameObject miniInfoPrefab;

        [Header("Component")]
        [SerializeField] private Transform listParent;

        // ����Ʈ �̴� ������ ���� ��ųʸ�(Ű: ����Ʈ�� �ν��Ͻ�ID, ��: ������Ʈ)
        private Dictionary<int, QuestMiniInfo> _questMiniDic;
        // ��� �� ���� ����� ť
        private Queue<QuestMiniInfo> _restingQuestMiniQueue;

        private QuestMiniInfo _tempInfo;

        private void Awake()
        {
            _questMiniDic = new Dictionary<int, QuestMiniInfo>();
            _restingQuestMiniQueue = new Queue<QuestMiniInfo>();

            Invoke("UpdateVerticalLayout", 1.2f);
        }

        public void AddMiniInfo(Quest quest)
        {
            // ���� ������� �̴� ������ ������ ����
            if (_restingQuestMiniQueue.Count > 0)
                _tempInfo = _restingQuestMiniQueue.Dequeue();
            // ���ο� �̴� ������ ����
            else
                _tempInfo = Instantiate(miniInfoPrefab, listParent).GetComponent<QuestMiniInfo>();

            // �̴� ������ ����Ʈ ���� �Ҵ�
            _tempInfo.Assign(quest);

            // ���� ��ųʸ��� �߰�
            _questMiniDic.Add(quest.GetInstanceID(), _tempInfo);

            // ���̾ƿ� ������Ʈ
            UpdateVerticalLayout();
        }

        public void RemoveMiniInfo(Quest quest)
        {
            // ��ųʸ����� Ž�� �� ����
            var instanceID = quest.GetInstanceID();
            _tempInfo = _questMiniDic[instanceID];
            _questMiniDic.Remove(instanceID);

            // �Ҵ� ���� �� ���� ��� ť�� �߰�
            _tempInfo.Unassign();
            _restingQuestMiniQueue.Enqueue(_tempInfo);
        }

        private void UpdateVerticalLayout()
        {
            Transform tempTr;
            Vector3 tempLocalPos;
            float yPos = -30;
            float gap = 5;

            foreach (KeyValuePair<int, QuestMiniInfo> miniInfo in _questMiniDic)
            {
                tempTr = miniInfo.Value.transform;
                tempLocalPos = tempTr.localPosition;
                tempLocalPos.y = yPos;
                tempTr.localPosition = tempLocalPos;
                yPos -= (tempTr as RectTransform).sizeDelta.y + gap;
            }
        }
    }
}
