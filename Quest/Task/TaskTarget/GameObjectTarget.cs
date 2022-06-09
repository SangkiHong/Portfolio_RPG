using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 타겟을 게임오브젝트 타입으로 지정하여 생성하는 모듈
     * 작성일: 22년 5월 19일
     */
    [CreateAssetMenu(menuName = "Quest/Task/Target/GameObject", fileName = "Target_")]
    public class GameObjectTarget : TaskTarget
    {
        [SerializeField] private GameObject value;

        public override object Value => value;

        public override bool IsEqual(object target)
        {
            // target을 GameObject형으로 캐스팅
            var targetAsGameObject = target as GameObject;

            // 같은 타입이 아닌 경우 캐스팅에 실패
            if (targetAsGameObject == null)
                return false;

            // 이름을 비교하여 타겟의 이름이 포함된 지에 대한 여부를 반환
            return targetAsGameObject.name.Contains(value.name);
        }
    }
}