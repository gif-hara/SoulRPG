namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleCharacterEvaluatorBoolean
    {
        bool Evaluate(BattleCharacter actor, BattleCharacter target);
    }
}
