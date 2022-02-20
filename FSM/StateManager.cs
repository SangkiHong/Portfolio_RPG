using System.Collections.Generic;
using UnityEngine;


namespace Sangki
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
            if (currentState != null)
            {
                //Run on exit actions of currentState
            }
            State targetState = GetState(targetId);
            
            //run on enter actions
            currentState = targetState;
            currentState.onEnter?.Invoke();
        }
        
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