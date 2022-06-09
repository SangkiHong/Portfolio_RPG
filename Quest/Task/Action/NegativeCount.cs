using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 성공 횟수가 음수면 카운트하여 반환하는 모듈
     * 작성일: 22년 5월 19일
     */
    [CreateAssetMenu(menuName = "Quest/Task/TaskAction/Negative Count", fileName = "NegativeCount")]
    public class NegativeCount : TaskAction
    {
        public override int Run(Task task, int currentSuccess, int successCount)
        {
            return successCount < 0 ? currentSuccess - successCount : currentSuccess;
        }
    }
}