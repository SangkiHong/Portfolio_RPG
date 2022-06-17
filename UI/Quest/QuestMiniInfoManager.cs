using System.Collections.Generic;
using UnityEngine;
using SK.Quests;

namespace SK.UI
{
    public class QuestMiniInfoManager : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject miniInfoPrefab;

        [Header("Component")]
        [SerializeField] private Transform listParent;

        // ����Ʈ �̴� ������ ���� ��ųʸ�
        private Dictionary<int, QuestMiniInfo> _questMiniDic;
        // ��� �� ���� ����� ť
        private Queue<QuestMiniInfo> _restingQuestMiniQueue;

        private QuestMiniInfo _tempInfo;

        private void Awake()
        {
            _questMiniDic = new Dictionary<int, QuestMiniInfo>();
            _restingQuestMiniQueue = new Queue<QuestMiniInfo>();
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
    }
}
