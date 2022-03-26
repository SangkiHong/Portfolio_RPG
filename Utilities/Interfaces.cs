using UnityEngine;

namespace SK
{
    public interface IDamagable
    {
        void OnDamage(int damageValue, Transform hitTransform, bool isCriticalHit);
    }

    public interface ITargetable
    { 
        
    }
}