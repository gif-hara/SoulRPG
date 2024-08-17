namespace SoulRPG.BattleSystems.BattleCharacterEvaluators
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBattleCharacterEvaluatorFloat
    {
        float Evaluate(BattleCharacter actor, BattleCharacter target);
    }
}
