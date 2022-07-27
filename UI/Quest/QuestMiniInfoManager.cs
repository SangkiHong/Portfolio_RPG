using System.Collections.Generic;
using UnityEngine;
using SK.Quests;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 플레이 화면에 표시될 퀘스트 미니 정보창 관리자 클래스
     * 작성일: 22년 6월 16일
     */

    public class QuestMiniInfoManager : MonoBehaviour
    {        
        [Header("Prefab")]
        [SerializeField] private GameObject miniInfoPrefab;

        [Header("Component")]
        [SerializeField] private Transform listParent;

        // 퀘스트 미니 인포를 담을 딕셔너리(키: 퀘스트의 인스턴스ID, 값: 컴포넌트)
        private Dictionary<int, QuestMiniInfo> _questMiniDic;
        // 사용 후 재사용 대기할 큐
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
            // 재사용 대기중인 미니 인포가 있으면 재사용
            if (_restingQuestMiniQueue.Count > 0)
                _tempInfo = _restingQuestMiniQueue.Dequeue();
            // 새로운 미니 인포를 생성
            else
                _tempInfo = Instantiate(miniInfoPrefab, listParent).GetComponent<QuestMiniInfo>();

            // 미니 인포에 퀘스트 정보 할당
            _tempInfo.Assign(quest);

            // 인포 딕셔너리에 추가
            _questMiniDic.Add(quest.GetInstanceID(), _tempInfo);

            // 레이아웃 업데이트
            UpdateVerticalLayout();
        }

        public void RemoveMiniInfo(Quest quest)
        {
            // 딕셔너리에서 탐색 후 제거
            var instanceID = quest.GetInstanceID();
            _tempInfo = _questMiniDic[instanceID];
            _questMiniDic.Remove(instanceID);

            // 할당 해제 후 재사용 대기 큐에 추가
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
