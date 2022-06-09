using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 업무 활동을 위한 모듈
     * 작성일: 22년 5월 19일
     */
    public abstract class TaskAction : ScriptableObject
    {
        public abstract int Run(Task task, int currentSuccess, int successCount);
    }
}