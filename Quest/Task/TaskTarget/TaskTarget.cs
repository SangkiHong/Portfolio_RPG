using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 시스템에 보고된 타겟이 Task에 설정한 타겟과 같은지 확인하는 모듈
     * 작성일: 22년 5월 19일
     */
    public abstract class TaskTarget : ScriptableObject
    {
        // 퀘스트 타겟이며 상속받는 자식 클래스에서 구현하기 때문에 object형으로 반환
        public abstract object Value { get; }

        // Task에 설정한 타겟과 같은지 여부를 반환하는 함수
        public abstract bool IsEqual(object target);
    }
}
