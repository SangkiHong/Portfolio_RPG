using UnityEngine;

namespace SK
{
    public abstract class Equipments : ScriptableObject
    {
        public GameObject modelPrefab;
        
        public virtual void ExecuteAction() { }

        public virtual void ExecuteAction(Animator anim, bool setDefault = false) { }
    }
}