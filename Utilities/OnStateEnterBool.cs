using UnityEngine;

namespace SK
{
    public class OnStateEnterBool : StateMachineBehaviour
    {
        public string boolName;
        public bool status;
        public bool resetOnExit;

        private int _boolHash;

        private void Awake() => _boolHash = Animator.StringToHash(boolName);
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            => animator.SetBool(_boolHash, status);
        

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (resetOnExit) animator.SetBool(_boolHash, !status);
        }
    }
}
