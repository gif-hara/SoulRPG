namespace SoulRPG.BattleSystems.DamageCalculators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDamageCalculator
    {
        int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon weapon);
    }
}
