namespace SK.FSM
{
    public abstract class StateBase
    {
        public virtual void StateInit() { }
        public virtual void FixedTick() { }
        public virtual void Tick() { }
        public virtual void StateExit(){ }
    }
}