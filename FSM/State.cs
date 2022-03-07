using System.Collections.Generic;

namespace SK.FSM
{
    public class State
    {
        public delegate void OnEnter();
        public OnEnter onEnter;
        
        private bool forceExit;
        List<StateAction> _fixedUpdateActions = new List<StateAction>();
        List<StateAction> _updateActions = new List<StateAction>();
        List<StateAction> _lateUpdateActions = new List<StateAction>();

        public State(List<StateAction> fixedUpdateActions, List<StateAction> updateActions, List<StateAction> lateUpdateActions)
        {
            this._fixedUpdateActions = fixedUpdateActions;
            this._updateActions = updateActions;
            this._lateUpdateActions = lateUpdateActions;
        }

        public void FixedTick()
        {
            ExecuteListOfActions(_fixedUpdateActions);
        }

        public void Tick()
        {
            ExecuteListOfActions(_updateActions);
        }

        public void LateTick()
        {
            ExecuteListOfActions(_lateUpdateActions);
            forceExit = false;
        }

        void ExecuteListOfActions(List<StateAction> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (forceExit)
                    return;
                
                forceExit = l[i].Execute();
            }
        }
    }
}
