using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 완료시 지급하는 보상에 관한 추상 클래스 모듈
     * 작성일: 22년 5월 20일
     */
    public abstract class Reward : ScriptableObject
    {
        [SerializeField] private uint quantity;

        public uint Quantity => quantity;

        public abstract void GiveReward();
    }
}