using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 플레이 화면에 표시되는 퀘스트 미니 정보창
     * 작성일: 22년 6월 16일
     */

    public class QuestMiniInfo : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image image_QuestIcon;
        [SerializeField] private Text text_QuestTitle;
        [SerializeField] private Text text_QuestDescription;

        private Quest _assignedQuest;
        private StringBuilder _infoStringBuilder;
        private TaskGroup _tempTaskGroup;

        private void Awake()
            => _infoStringBuilder = new StringBuilder();

        public void Assign(Quest quest)
        {
            // 할당된 퀘스트를 변수에 저장
            _assignedQuest = quest;

            // 퀘스트 정보 UI 할당
            text_QuestTitle.text = quest.DisplayName;
            UpdateInfoText(quest);

            transform.SetAsLastSibling();
            gameObject.SetActive(true);

            // 퀘스트 완수가 되면 업데이트를 하도록 이벤트에 함수 등록
            quest.OnUpdateQuestState += UpdateInfoText;
        }

        public void Unassign()
        {
            text_QuestTitle.text = string.Empty;
            text_QuestDescription.text = string.Empty;
            gameObject.SetActive(false);
        }

        // 퀘스트 상태가 변경되었다면 호출될 함수_220618
        private void UpdateInfoText(Quest quest)
        {
            // 변경된 퀘스트가 할당된 퀘스트와 같다면 상태 정보 변경
            if (_assignedQuest == quest)
            {
                // 스트링빌더 초기화
                _infoStringBuilder.Clear();
                _tempTaskGroup = quest.CurrentTaskGroup;
                for (int i = 0; i < _tempTaskGroup.Tasks.Count; i++)
                {
                    // 스트링 빌딩(예: "- 업무 설명 요약 (현재 성공 횟수 / 필요 성공 횟수)")
                    if (i > 0) _infoStringBuilder.AppendLine(); // 2번째줄부터 줄바꿈
                    _infoStringBuilder.Append(Strings.QuestMiniInfo_Bar);
                    _infoStringBuilder.Append(_tempTaskGroup.Tasks[i].Description);
                    _infoStringBuilder.Append(Strings.QuestMiniInfo_OpenBraket);

                    // 퀘스트가 완료되지 않았다면
                    if (!_assignedQuest.IsCompletable)
                    {
                        _infoStringBuilder.Append(_tempTaskGroup.Tasks[i].CurrentSuccess);
                        _infoStringBuilder.Append(Strings.QuestMiniInfo_Slash);
                        _infoStringBuilder.Append(_tempTaskGroup.Tasks[i].NeedSuccessToComplete);
                    }
                    // 퀘스트가 완료된 경우
                    else
                        _infoStringBuilder.Append(Strings.QuestMiniInfo_Success);

                    _infoStringBuilder.Append(Strings.QuestMiniInfo_CloseBraket);
                }

                text_QuestDescription.text = _infoStringBuilder.ToString();
            }
        }
    }
}