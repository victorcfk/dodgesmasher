public interface Damageable
{
    void TakeDamage(int inDmg);
    bool IsDamageable { get; }
    UnityEngine.GameObject DamageableObject { get; }
}
