using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ �ʱ� ���� ���� �����ϴ� ���
     * �ۼ���: 22�� 5�� 19��
     */
    public abstract class InitialSuccessValue : ScriptableObject
    {
        public abstract int GetValue(Task task);
    }
}