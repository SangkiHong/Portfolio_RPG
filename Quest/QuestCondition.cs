using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 진행 가능한 요구 조건을 지정하는 모듈
     * 작성일: 22년 5월 20일
     */

    public abstract class QuestCondition : ScriptableObject
    {
        [SerializeField] private string description;
        public abstract bool IsPass(Quest quest);
    }
}