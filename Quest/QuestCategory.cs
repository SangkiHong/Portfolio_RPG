using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ ī�װ��� �����ϴ� ���
     * �ۼ���: 22�� 5�� 19��
     */

    public enum Category
    {
        MainQuest, // �� ����Ʈ
        SubQuest, // �ΰ� ����Ʈ
        GuildQuest // ��� ����Ʈ
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

        // string���� �ٷ� �� �� �� �ֵ��� �� ������ ����
        public static bool operator ==(QuestCategory lhs, QuestCategory rhs)
        {
            if (lhs is null) // lhs�� ���̸� ���۷��� �� ��ȯ
                return ReferenceEquals(rhs, null);
            // �� �� null�� �ƴ� ���
            return lhs.questCategory == rhs.questCategory && lhs.codeName == rhs.codeName;
        }

        // ��Ī ������
        public static bool operator !=(QuestCategory lhs, QuestCategory rhs) => !(lhs == rhs);
        #endregion
    }
}
