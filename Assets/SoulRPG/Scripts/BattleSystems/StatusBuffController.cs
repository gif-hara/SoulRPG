using System.Collections.Generic;
using System.Linq;
using SoulRPG.BattleSystems.BattleCharacterEvaluators;
using UnityEngine;
using UnityEngine.Assertions;
using UnitySequencerSystem;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class StatusBuffController
    {
        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> physicalStrengthBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> magicalStrengthBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> slashCutRateBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> blowCutRateBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> thrustCutRateBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> magicCutRateBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> fireCutRateBuffList = new();

        private readonly List<(string id, float rate, IBattleCharacterEvaluatorBoolean condition)> thunderCutRateBuffList = new();

        public void Add(IEnumerable<Define.StatusType> statusTypes, string name, float rate, IBattleCharacterEvaluatorBoolean condition)
        {
            foreach (var statusType in statusTypes)
            {
                Add(statusType, name, rate, condition);
            }
        }

        public void Add(Define.StatusType statusType, string name, float rate, IBattleCharacterEvaluatorBoolean condition)
        {
            var buffList = statusType switch
            {
                Define.StatusType.PhysicalStrength => physicalStrengthBuffList,
                Define.StatusType.MagicalStrength => magicalStrengthBuffList,
                Define.StatusType.SlashCutRate => slashCutRateBuffList,
                Define.StatusType.BlowCutRate => blowCutRateBuffList,
                Define.StatusType.ThrustCutRate => thrustCutRateBuffList,
                Define.StatusType.MagicCutRate => magicCutRateBuffList,
                Define.StatusType.FireCutRate => fireCutRateBuffList,
                Define.StatusType.ThunderCutRate => thunderCutRateBuffList,
                _ => throw new System.ArgumentOutOfRangeException(statusType.ToString())
            };
            if (buffList.Any(x => x.id == name))
            {
                return;
            }
            buffList.Add((name, rate, condition));
        }

        public void Remove(string name)
        {
            physicalStrengthBuffList.RemoveAll(x => x.id == name);
            magicalStrengthBuffList.RemoveAll(x => x.id == name);
            slashCutRateBuffList.RemoveAll(x => x.id == name);
            blowCutRateBuffList.RemoveAll(x => x.id == name);
            thrustCutRateBuffList.RemoveAll(x => x.id == name);
            magicCutRateBuffList.RemoveAll(x => x.id == name);
            fireCutRateBuffList.RemoveAll(x => x.id == name);
            thunderCutRateBuffList.RemoveAll(x => x.id == name);
        }

        public float GetStrengthRate(Define.AttackType attackType, BattleCharacter actor, BattleCharacter target, Container container)
        {
            var buffList = attackType switch
            {
                Define.AttackType.Physical => physicalStrengthBuffList,
                Define.AttackType.Magical => magicalStrengthBuffList,
                _ => throw new System.ArgumentOutOfRangeException(attackType.ToString())
            };
            var result = 1.0f;
            foreach (var (_, rate, condition) in buffList)
            {
                if (condition != null && !condition.Evaluate(actor, target, container))
                {
                    continue;
                }
                result *= rate;
            }
            return result;
        }

        public float GetCutRate(Define.AttackAttribute attackAttribute, BattleCharacter actor, BattleCharacter target, Container container)
        {
            var list = attackAttribute switch
            {
                Define.AttackAttribute.Slash => slashCutRateBuffList,
                Define.AttackAttribute.Blow => blowCutRateBuffList,
                Define.AttackAttribute.Thrust => thrustCutRateBuffList,
                Define.AttackAttribute.Magic => magicCutRateBuffList,
                Define.AttackAttribute.Fire => fireCutRateBuffList,
                Define.AttackAttribute.Thunder => thunderCutRateBuffList,
                _ => throw new System.ArgumentOutOfRangeException(attackAttribute.ToString())
            };
            return list
                .Where(x => x.condition == null || x.condition.Evaluate(actor, target, container))
                .Select(x => x.rate).Sum();
        }
    }
}
