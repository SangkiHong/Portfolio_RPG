using System.Collections.Generic;
using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EffectData_", menuName = "Game Data/EffectData")]
    public class EffectData : ScriptableObject
    {
        public List<EffectPoolList> effectList;

        public void CopyData(ref List<EffectPoolList> sourceData)
        {
            effectList = new List<EffectPoolList>();

            for (int i = 0; i < sourceData.Count; i++)
            {
                effectList.Add(sourceData[i]);
            }
        }
    }
}