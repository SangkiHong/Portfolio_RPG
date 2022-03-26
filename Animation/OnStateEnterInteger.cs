using UnityEngine;
using UnityEngine.Animations;

namespace SK
{
    public class OnStateEnterInteger : StateMachineBehaviour
    {
        public string integerName;
        public int setValue;
        public bool random;
        public int maxValue;
        private int _boolHash, _value;

        private void Awake() => _boolHash = Animator.StringToHash(integerName);
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (random) 
                _value = Random.Range(0, maxValue + 1);
            else
                _value = setValue;
            
            animator.SetInteger(_boolHash, _value);
        }
    }
}