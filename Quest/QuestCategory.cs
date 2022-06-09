using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 카테고리를 지정하는 모듈
     * 작성일: 22년 5월 19일
     */

    public enum Category
    {
        MainQuest, // 주 퀘스트
        SubQuest, // 부가 퀘스트
        GuildQuest // 길드 퀘스트
    }

    [CreateAssetMenu(menuName = "Quest/Category", fileName = "Category_")]
    public class QuestCategory : ScriptableObject, IEquatable<QuestCategory>
    {
        [SerializeField] internal Category questCategory;
        [SerializeField] private string codeName;
        [SerializeField] private string displayName;

        public string CodeName => codeName;
        public string DisplayName => displayName;

        #region Operator
        public bool Equals(QuestCategory other)
        {
            if (other is null) return false;

            if (ReferenceEquals(other, this)) return true;
            
            if (GetType() != other.GetType()) return false;

            return codeName == other.CodeName;
        }

        public override int GetHashCode() => (CodeName, DisplayName).GetHashCode();

        public override bool Equals(object other) => base.Equals(other);

        // string형과 바로 비교 할 수 있도록 비교 연산자 구현
        public static bool operator ==(QuestCategory lhs, QuestCategory rhs)
        {
            if (lhs is null) // lhs가 널이면 레퍼런스 비교 반환
                return ReferenceEquals(rhs, null);
            // 둘 다 null이 아닌 경우
            return lhs.questCategory == rhs.questCategory && lhs.codeName == rhs.codeName;
        }

        // 대칭 연산자
        public static bool operator !=(QuestCategory lhs, QuestCategory rhs) => !(lhs == rhs);
        #endregion
    }
}
