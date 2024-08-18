namespace SoulRPG.BattleSystems.RecoveryCalculators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRecoveryCalculator
    {
        int Calculate(BattleCharacter attacker, BattleCharacter defender, MasterData.Weapon attackerWeapon, Define.TargetType targetType);
    }
}
