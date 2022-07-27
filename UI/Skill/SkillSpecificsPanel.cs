using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class SkillSpecificsPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image_skillIcon;
        [SerializeField] private Text text_InfoSkillPoint;
        [SerializeField] private Text text_RequireSkillPoint;
        [SerializeField] private Text text_SkillName;
        [SerializeField] private Text text_SkillType;
        [SerializeField] private Text text_PrevSkill;
        [SerializeField] private Text text_SkillAmountMp;
        [SerializeField] private Text text_SkillCoolTime;
        [SerializeField] private Text text_SkillDescription;

        private Data.SkillData _assignedSkill;

        private StringBuilder _stringBuilder;

        private readonly Color _color_ReadyActivation = new Color(.22f, .98f, .396f, 1);
        private readonly Color _color_LackPoint = new Color(1f, .32f, .14f, 1);

        private readonly string _string_ActivateSkill = "��Ŭ���Ͽ� ��ų ����";
        private readonly string _string_NeedPrevSkillActivation = "���� ��ų ���� �ʿ�";
        private readonly string _string_LackSkillPoint = "��ų ����Ʈ ����";
        private readonly string _string_AmountUsingMp = "MP ��뷮: ";
        private readonly string _string_RequireSkillPoint = "�ʿ� ��ų ����Ʈ: ";
        private readonly string _string_PrevSkill = "- ���� ��ų : ";
        private readonly string _string_CoolTime = "- ���� ���ð� : ";
        private readonly string _string_Second = "��";
        private readonly char _string_Comma = ',';
        private readonly char _string_NewLine = '\n';

        public void AssignSkill(Data.SkillData skillData, PointerEventData eventData, bool isActivated)
        {
            gameObject.SetActive(true);

            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder();

            rectTransform.position = eventData.position;

            // �Ҵ�� ��ų�� �ٸ��ٸ� ��ų ���� �Ҵ�
            if (_assignedSkill != skillData)
            {
                // ��Ʈ������ �ʱ�ȭ
                _stringBuilder.Clear();

                // ��ų ������ ������ �Ҵ�
                _assignedSkill = skillData;

                // ��Ȱ��ȭ ��ų�� ��� �ʿ� ��ų ����Ʈ ǥ��
                if (!isActivated)
                {
                    var requirePoint = skillData.requireSkillPoint;

                    text_InfoSkillPoint.gameObject.SetActive(true);

                    // ���� ��ų�� �������� ���� ��� ���� ��ų ���� �ʿ� �ȳ� ǥ��
                    if (skillData.prevSkill != null && !UIManager.Instance.skillManager.IsActivated(skillData.prevSkill))
                    {
                        text_InfoSkillPoint.text = _string_NeedPrevSkillActivation;
                        text_InfoSkillPoint.color = _color_LackPoint;
                    }
                    // ��ų ����Ʈ ���� �ȳ� ǥ��
                    else if (GameManager.Instance.DataManager.PlayerData.SkillPoint < requirePoint)
                    {
                        text_InfoSkillPoint.text = _string_LackSkillPoint;
                        text_InfoSkillPoint.color = _color_LackPoint;
                    }
                    // ��ų ����Ʈ ��� �ȳ� ǥ��
                    else
                    {
                        text_InfoSkillPoint.text = _string_ActivateSkill;
                        text_InfoSkillPoint.color = _color_ReadyActivation;
                    }

                    // �ʿ� ��ų ����Ʈ ǥ��
                    text_RequireSkillPoint.gameObject.SetActive(true);
                    _stringBuilder.Append(_string_RequireSkillPoint);
                    _stringBuilder.Append(requirePoint);
                    text_RequireSkillPoint.text = _stringBuilder.ToString();
                }
                else
                {
                    text_InfoSkillPoint.gameObject.SetActive(false);
                    text_RequireSkillPoint.gameObject.SetActive(false);
                }
                // ��ų ������
                image_skillIcon.sprite = skillData.skillIcon;

                // ��ų �̸�
                text_SkillName.text = skillData.skillName;

                // ��ų Ÿ��
                text_SkillType.text = skillData.isActiveSkill ? Strings.Skill_ActiveSkill : Strings.Skill_Passive;

                // ���� ��ų ����
                if (skillData.prevSkill != null)
                {
                    _stringBuilder.Clear();
                    _stringBuilder.Append(_string_PrevSkill);
                    _stringBuilder.Append(skillData.prevSkill.skillName);
                    text_PrevSkill.text = _stringBuilder.ToString();
                    text_PrevSkill.gameObject.SetActive(true);
                }
                else
                    text_PrevSkill.gameObject.SetActive(false);

                // MP ��뷮
                _stringBuilder.Clear();
                _stringBuilder.Append(_string_AmountUsingMp);
                _stringBuilder.Append(skillData.useMpAmount);
                text_SkillAmountMp.text = _stringBuilder.ToString();

                // ��ų ���� �ð�
                _stringBuilder.Clear();
                _stringBuilder.Append(_string_CoolTime);
                _stringBuilder.Append(skillData.skillCoolTime);
                _stringBuilder.Append(_string_Second);
                text_SkillCoolTime.text = _stringBuilder.ToString();
                string skillDesc = skillData.skillDescription.Replace(_string_Comma, _string_NewLine);
                text_SkillDescription.text = skillDesc;
            }
        }

        // ��ų Ȱ��ȭ�� ���� ����
        public void UpdateSkillState()
        {
            text_InfoSkillPoint.gameObject.SetActive(false);
            text_RequireSkillPoint.gameObject.SetActive(false);
        }
    }
}
