using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    [CreateAssetMenu(menuName = "Quest/Task/TaskAction/SimpleCount", fileName = "Simple Count")]
    public class SimpleCount : TaskAction
    {
        public override int Run(Task task, int currentSuccess, int successCount)
        {
            return currentSuccess + successCount;
        }
    }
}