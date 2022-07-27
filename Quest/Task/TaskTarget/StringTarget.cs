using System.Collections;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 타겟을 스트링 타입으로 지정하여 생성하는 모듈
     * 작성일: 22년 5월 19일
     */
    [CreateAssetMenu(menuName = "Quest/Task/Target/String", fileName = "Target_")]
    public class StringTarget : TaskTarget
    {
        [SerializeField] private string value;

        public override object Value => value;

        public override bool IsEqual(object target)
        {
            // target을 string형으로 캐스팅
            string targetAsString = target as string;

            Debug.Log($"value: {value}, targetAsString: {targetAsString}");

            // 같은 타입이 아닌 경우 캐스팅에 실패
            if (targetAsString == null)
                return false;

            Debug.Log($"Equal Result: {value == targetAsString || targetAsString.Contains(value)}");
            return value == targetAsString || targetAsString.Contains(value);
        }
    }
}