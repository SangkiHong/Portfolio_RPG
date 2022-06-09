using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ �ý��ۿ� ����� Ÿ���� Task�� ������ Ÿ�ٰ� ������ Ȯ���ϴ� ���
     * �ۼ���: 22�� 5�� 19��
     */
    public abstract class TaskTarget : ScriptableObject
    {
        // ����Ʈ Ÿ���̸� ��ӹ޴� �ڽ� Ŭ�������� �����ϱ� ������ object������ ��ȯ
        public abstract object Value { get; }

        // Task�� ������ Ÿ�ٰ� ������ ���θ� ��ȯ�ϴ� �Լ�
        public abstract bool IsEqual(object target);
    }
}
