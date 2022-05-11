using UnityEngine;

namespace SK
{
    public interface IDamagable
    {
        void OnDamage(uint damageValue, Transform hitTransform, bool isCriticalHit, bool isStrongAttack);
    }

    public interface ITargetable
    { 
        
    }
}