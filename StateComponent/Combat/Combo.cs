using UnityEngine;

namespace SK.Behavior
{
    /*
        �ۼ���: ȫ���
        ����: �÷��̾��� �޺� ���ݿ� ���� ������ �޾� ���� ������ ��ȯ���ִ� ������Ʈ
        �ۼ���: 22�� 5�� 29��
    */
    public enum AttackType
    {
        CounterAttack, DodgeAttack, ChargeAttack, FinishAttack
    }

    public class Combo : MonoBehaviour
    {
        // Ʈ�� ������ �޺� ���
        private class ComboTreeNode
        {
            public Attack attack;
            public ComboTreeNode aCombo;
            public ComboTreeNode bCombo;
        }

        // �޺� ����Ʈ ����ü
        [System.Serializable]
        public struct ComboList
        {
            public string combo;
            public Attack attack;
        }

        // Ư�� ���� ����ü
        [System.Serializable]
        public struct SpecialAttack
        {
            public AttackType attackType;
            public Attack[] specialAttack;
        }

        // �޺� ���� �ð�
        [SerializeField] private float comboIntervalTime;

        // �޺� ����Ʈ
        [SerializeField] private ComboList[] comboList;

        [Header("Special Attack")]
        [SerializeField] private SpecialAttack[] specialAttacks;

        // �޺� Ʈ�� ��Ʈ
        private ComboTreeNode _rootNode;
        // ���� �޺���� ��ġ
        private ComboTreeNode _currentCombo;

        private readonly string _methodName = "CancelCombo";
        private readonly string aKey = "A";
        private readonly string bKey = "B";
        private readonly char _comma = ',';

        private void Awake()
        {
            // ��Ʈ ��� �ʱ�ȭ
            _rootNode = new ComboTreeNode();
            _currentCombo = _rootNode;

            // �޺� ����Ʈ ����
            for (int i = 0; i < comboList.Length; i++)
                SetCombo(comboList[i]);
        }

        private void SetCombo(ComboList comboList)
        {
            ComboTreeNode targetComboNode = _rootNode;

            string[] combo = comboList.combo.Split(_comma);

            // Ÿ�� �޺� ��带 Ž��
            for (int i = 0; i < combo.Length; i++)
            {
                if (combo[i] == aKey)
                {
                    if (targetComboNode.aCombo == null)
                        targetComboNode.aCombo = new ComboTreeNode();

                    targetComboNode = targetComboNode.aCombo;
                }
                else if (combo[i] == bKey)
                {
                    if (targetComboNode.bCombo == null)
                        targetComboNode.bCombo = new ComboTreeNode();

                    targetComboNode = targetComboNode.bCombo;
                }
            }

            targetComboNode.attack = comboList.attack;
        }

        public Attack GetCombo(bool isLeft)
        {
            // ��Ŭ���� ���� �޺� ������ ���
            if (isLeft)
            {
                // aCombo ��尡 �ִ� ���
                if (_currentCombo.aCombo != null)
                    _currentCombo = _currentCombo.aCombo;
                else
                    _currentCombo = _rootNode.aCombo;
            }
            // ��Ŭ���� ���� �޺� ������ ���
            else
            {
                // bCombo ��尡 �ִ� ���
                if (_currentCombo.bCombo != null)
                    _currentCombo = _currentCombo.bCombo;
                else
                    _currentCombo = _rootNode.bCombo;
            }

            if (_currentCombo == null)
                return null;

            CancelInvoke();
            Invoke(_methodName, comboIntervalTime);
            return _currentCombo.attack;
        }

        public Attack ExecuteSpecialAttack(AttackType attackType, int index = 0)
        {
            foreach (var attack in specialAttacks)
                if (attack.attackType.Equals(attackType))
                    return attack.specialAttack[index];

            return null;
        }

        private void CancelCombo() => _currentCombo = _rootNode;
    }
}
