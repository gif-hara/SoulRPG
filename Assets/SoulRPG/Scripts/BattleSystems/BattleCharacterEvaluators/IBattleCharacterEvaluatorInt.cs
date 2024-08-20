namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleCharacterEvaluatorInt
    {
        int Evaluate(BattleCharacter actor, BattleCharacter target);
    }
}
