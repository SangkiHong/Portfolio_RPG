using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 성공 횟수가 양수면 카운트하고 음수면 0으로 초기화하여 반환하는 모듈
     * 작성일: 22년 5월 19일
     */
    [CreateAssetMenu(menuName = "Quest/Task/TaskAction/Continuous Count", fileName = "ContinuousCount")]
    public class ContinuousCount : TaskAction
    {
        public override int Run(Task task, int currentSuccess, int successCount)
        {
            return successCount > 0 ? currentSuccess + successCount : 0;
        }
    }
}