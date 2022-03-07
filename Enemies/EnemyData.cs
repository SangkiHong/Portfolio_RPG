
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyName;
    [SerializeField] private int level;
    [SerializeField] private int hp;
    [SerializeField] private int mp;
    [SerializeField] private int str;
    [SerializeField] private int def;
    [SerializeField] private float speed;

    public string EnemyName => enemyName;
    public int Level => level;
    public int Hp => hp;
    public int Mp => mp;
    public int Str => str;
    public int Def => def;
    public float Speed => speed;

}
