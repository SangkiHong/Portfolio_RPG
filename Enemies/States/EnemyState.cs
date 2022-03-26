namespace SK.FSM
{
    public abstract class EnemyState
    {
        public virtual void StateInit() { }
        public virtual void FixedTick() { }
        public virtual void Tick() { }
        public virtual void LateTick(){ }
        public virtual void StateExit(){ }
    }
}