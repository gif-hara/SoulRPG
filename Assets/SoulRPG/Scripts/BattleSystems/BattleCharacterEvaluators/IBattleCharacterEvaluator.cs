namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleCharacterEvaluator
    {
        bool Evaluate(BattleCharacter actor, BattleCharacter target);
    }
}
