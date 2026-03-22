using UnityEngine;

[CreateAssetMenu(fileName = "BigGhostStat", menuName = "Scriptable Objects/BigGhostStat")]
public class BigGhostStat : ScriptableObject
{
    public float MaxHP;
    public float AttackPower;
    public float ChaseRange;
    public float AggroRange;
    public float AttackRange;
    public float AttackSpeed;
    public float SpacingSpeed;
    public float Speed;
}
