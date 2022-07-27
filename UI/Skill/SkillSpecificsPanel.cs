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

        private readonly string _string_ActivateSkill = "우클릭하여 스킬 해제";
        private readonly string _string_NeedPrevSkillActivation = "선행 스킬 해제 필요";
        private readonly string _string_LackSkillPoint = "스킬 포인트 부족";
        private readonly string _string_AmountUsingMp = "MP 사용량: ";
        private readonly string _string_RequireSkillPoint = "필요 스킬 포인트: ";
        private readonly string _string_PrevSkill = "- 선행 스킬 : ";
        private readonly string _string_CoolTime = "- 재사용 대기시간 : ";
        private readonly string _string_Second = "초";
        private readonly char _string_Comma = ',';
        private readonly char _string_NewLine = '\n';

        public void AssignSkill(Data.SkillData skillData, PointerEventData eventData, bool isActivated)
        {
            gameObject.SetActive(true);

            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder();

            rectTransform.position = eventData.position;

            // 할당된 스킬과 다르다면 스킬 정보 할당
            if (_assignedSkill != skillData)
            {
                // 스트링빌더 초기화
                _stringBuilder.Clear();

                // 스킬 데이터 변수에 할당
                _assignedSkill = skillData;

                // 비활성화 스킬인 경우 필요 스킬 포인트 표시
                if (!isActivated)
                {
                    var requirePoint = skillData.requireSkillPoint;

                    text_InfoSkillPoint.gameObject.SetActive(true);

                    // 선행 스킬이 해제되지 않은 경우 선행 스킬 해제 필요 안내 표시
                    if (skillData.prevSkill != null && !UIManager.Instance.skillManager.IsActivated(skillData.prevSkill))
                    {
                        text_InfoSkillPoint.text = _string_NeedPrevSkillActivation;
                        text_InfoSkillPoint.color = _color_LackPoint;
                    }
                    // 스킬 포인트 부족 안내 표시
                    else if (GameManager.Instance.DataManager.PlayerData.SkillPoint < requirePoint)
                    {
                        text_InfoSkillPoint.text = _string_LackSkillPoint;
                        text_InfoSkillPoint.color = _color_LackPoint;
                    }
                    // 스킬 포인트 사용 안내 표시
                    else
                    {
                        text_InfoSkillPoint.text = _string_ActivateSkill;
                        text_InfoSkillPoint.color = _color_ReadyActivation;
                    }

                    // 필요 스킬 포인트 표시
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
                // 스킬 아이콘
                image_skillIcon.sprite = skillData.skillIcon;

                // 스킬 이름
                text_SkillName.text = skillData.skillName;

                // 스킬 타입
                text_SkillType.text = skillData.isActiveSkill ? Strings.Skill_ActiveSkill : Strings.Skill_Passive;

                // 선행 스킬 정보
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

                // MP 사용량
                _stringBuilder.Clear();
                _stringBuilder.Append(_string_AmountUsingMp);
                _stringBuilder.Append(skillData.useMpAmount);
                text_SkillAmountMp.text = _stringBuilder.ToString();

                // 스킬 재사용 시간
                _stringBuilder.Clear();
                _stringBuilder.Append(_string_CoolTime);
                _stringBuilder.Append(skillData.skillCoolTime);
                _stringBuilder.Append(_string_Second);
                text_SkillCoolTime.text = _stringBuilder.ToString();
                string skillDesc = skillData.skillDescription.Replace(_string_Comma, _string_NewLine);
                text_SkillDescription.text = skillDesc;
            }
        }

        // 스킬 활성화로 정보 변경
        public void UpdateSkillState()
        {
            text_InfoSkillPoint.gameObject.SetActive(false);
            text_RequireSkillPoint.gameObject.SetActive(false);
        }
    }
}
