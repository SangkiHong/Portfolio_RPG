using UnityEngine;

namespace SK
{
    public abstract class Equipments : ScriptableObject
    {
        public GameObject modelPrefab;
        
        public virtual void ExcuteAction()
        {
        }
    }
}