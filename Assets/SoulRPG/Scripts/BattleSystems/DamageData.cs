namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public struct DamageData
    {
        public int Damage;

        public Define.AttackAttribute AttackAttribute;

        public DamageData(int damage, Define.AttackAttribute attackAttribute)
        {
            Damage = damage;
            AttackAttribute = attackAttribute;
        }
    }
}
