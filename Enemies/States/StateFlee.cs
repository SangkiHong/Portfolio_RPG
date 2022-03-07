namespace SK.FSM
{
    public class StateFlee : EnemyState
    {
        private readonly Enemy _enemy;
        public StateFlee(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }
    }
}