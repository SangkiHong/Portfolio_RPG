using System.Collections.Generic;
using UnityEngine;

namespace SK.Data
{
    [System.Serializable]
    public class ItemData
    {
        public Item item; // 아이템 정보
        public int slotID; // 슬롯 ID 정보
        public uint amount; // 아이템 갯수
        public bool isEquiped; // 아이템 착용 여부
    }

    [CreateAssetMenu(fileName = "PlayerItemData", menuName = "Game Data/Player Item Data")]
    public class PlayerItemData : ScriptableObject
    {
        [SerializeField] public List<ItemData> items; // 플레이어 아이템 정보 리스트_220504

        private ItemData tempItemData;

        // 슬롯에 아이템을 새로 추가_220505
        public void AddItem(Item slotItem, int slotID, uint amount)
        {
            // 아이템이 수량 중첩이 가능한 경우
            if (slotItem.isStackable)
            {
                foreach (var item in items)
                {
                    if (item.item.Equals(slotItem))
                    {
                        item.amount += amount;
                        return;
                    }
                }
            }
            else
            {
                // 아이템 데이터 생성 및 추가            
                items.Add(new ItemData
                {
                    item = slotItem,
                    amount = amount,
                    slotID = slotID
                });
            }
        }

        // 아이템과 슬롯 ID를 비교하여 리스트에서 삭제_220504
        public void RemoveItem(Item slotItem, int slotID)
        {
            for (int i = 0; i < items.Count; i++)
            {
                // 슬롯 ID와 아이템이 같으면 해당 데이터 삭제
                if (items[i].slotID == slotID && items[i].item.Equals(slotItem))
                { 
                    items.RemoveAt(i); 

                }
            }
        }

        // 아이템과 아이템 수량을 비교하여 리스트에서 삭제_220504
        public void RemoveItem(Item item, uint amount)
        {
            for (int i = 0; i < items.Count; i++)
            {
                // 슬롯 ID와 아이템이 같으면 해당 데이터 삭제
                if (items[i].item.Equals(item) && items[i].amount == amount)
                { 
                    items.RemoveAt(i); 
                    return;
                }
            }
        }

        // 두 슬롯 데이터의 슬롯 ID 교환_220504
        public void SwapSlotData(int aSlotID, int bSlotID)
        {
            // B데이터를 임시 데이터 변수에 저장
            tempItemData = items.Find(x => x.slotID == bSlotID);

            // B데이터의 슬롯 ID가 매칭이 안된 경우(빈 슬롯)
            if (tempItemData == null)
            {
                // A데이터의 슬롯 ID를 B데이터 ID로 단순 변경
                items.Find(x => x.slotID == aSlotID).slotID = bSlotID;
            }
            // 두 데이터를 교환
            else
            {
                // 인덱스 값 -1로 초기화
                int aIndex = -1, bIndex = -1;

                // A, B데이터의 리스트 인덱스 찾기
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].slotID == aSlotID) aIndex = i;
                    if (items[i].slotID == bSlotID) bIndex = i;
                    if (aIndex > -1 && bIndex > -1) break;
                }

                // B데이터에 A슬롯 데이터를 대입
                items[bIndex] = items[aIndex];
                // B데이터의 ID 유지
                items[bIndex].slotID = bSlotID;

                // A데이터에 임시 데이터 변수(B데이터)를 대입
                items[aIndex] = tempItemData;
                // A데이터의 ID 유지
                items[aIndex].slotID = aSlotID;
            }
        }

        // 해당 슬롯 데이터의 아이템 수량 변경_220507
        public void ChangeSlotInfo(int slotID, uint changeAmount)
        {
            // 슬롯ID를 통해 검색한 데이터의 아이템 수량을 변경
            tempItemData = items.Find(x => x.slotID == slotID);

            if (tempItemData != null) tempItemData.amount = changeAmount;
        }

        // 해당 슬롯 데이터의 아이템 수량 변경_220507
        public void ChangeSlotInfo(Item item, uint currentAmount, uint changeAmount)
        {
            // 아이템 정보를 통해 검색한 데이터의 아이템 수량을 변경
            items.Find(x => x.item.Equals(item) && x.amount == currentAmount).amount = changeAmount;
        }

        // 해당 슬롯 데이터의 아이템 착용 상태 변경_220512
        public void ChangeSlotInfo(Item item, bool equip)
        {
            // 아이템 정보를 통해 검색한 데이터의 아이템 착용 상태를 변경
            tempItemData = items.Find(x => x.item.Equals(item) && x.isEquiped != equip);
            
            if (tempItemData != null)tempItemData.isEquiped = equip;
        }
    }
}