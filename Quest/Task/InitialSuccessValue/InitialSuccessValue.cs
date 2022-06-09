using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 초기 설정 값을 지정하는 모듈
     * 작성일: 22년 5월 19일
     */
    public abstract class InitialSuccessValue : ScriptableObject
    {
        public abstract int GetValue(Task task);
    }
}