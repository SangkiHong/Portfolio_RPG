using System.Collections.Generic;
using UnityEngine;


namespace SK.FSM
{
    public abstract class StateManager : MonoBehaviour
    {
        private State currentState;
        private Dictionary<string, State> allStates = new Dictionary<string, State>();
        
        [HideInInspector]
        public Transform mTransform;
        
        private void Start()
        {
            mTransform = this.transform;
            Init();
        }

        public abstract void Init();

        public void FixedTick()
        {
            if (currentState == null) return;
            
            currentState.FixedTick();
        }

        public void Tick()
        {
            if (currentState == null) return;
            
            currentState.Tick();
        }

        public void LateTick()
        {
            if (currentState == null) return;
            
            currentState.LateTick();
        }

        public void ChangeState(string targetId)
        {
            //if (currentState != null)            
            //    currentState.onExit?.Invoke();
            
            State targetState = GetState(targetId);
            if (currentState == targetState) return;
            
            //run on enter actions
            currentState = targetState;
            currentState.onEnter?.Invoke();
        }

        public State CurrentState() => currentState;

        State GetState(string targetId)
        {
            allStates.TryGetValue(targetId, out State retVal);
            return retVal;
        }

        protected void RegisterState(string stateId, State state)
        {
            allStates.Add(stateId, state);
        }
    }
}