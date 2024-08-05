using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class StatusBuffController
    {
        private readonly List<(string, float)> physicalStrengthBuffList = new();

        private readonly List<(string, float)> magicalStrengthBuffList = new();

        private readonly List<(string, float)> slashCutRateBuffList = new();

        private readonly List<(string, float)> blowCutRateBuffList = new();

        private readonly List<(string, float)> thrustCutRateBuffList = new();

        private readonly List<(string, float)> magicCutRateBuffList = new();

        private readonly List<(string, float)> fireCutRateBuffList = new();

        private readonly List<(string, float)> thunderCutRateBuffList = new();

        public void Add(IEnumerable<Define.StatusType> statusTypes, string name, float rate)
        {
            foreach (var statusType in statusTypes)
            {
                Add(statusType, name, rate);
            }
        }

        public void Add(Define.StatusType statusType, string name, float rate)
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
            if (buffList.Any(x => x.Item1 == name))
            {
                return;
            }
            buffList.Add((name, rate));
        }

        public void Remove(string name)
        {
            physicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
            magicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
            slashCutRateBuffList.RemoveAll(x => x.Item1 == name);
            blowCutRateBuffList.RemoveAll(x => x.Item1 == name);
            thrustCutRateBuffList.RemoveAll(x => x.Item1 == name);
            magicCutRateBuffList.RemoveAll(x => x.Item1 == name);
            fireCutRateBuffList.RemoveAll(x => x.Item1 == name);
            thunderCutRateBuffList.RemoveAll(x => x.Item1 == name);
        }

        public float GetStrengthRate(Define.AttackType attackType)
        {
            var buffList = attackType switch
            {
                Define.AttackType.Physical => physicalStrengthBuffList,
                Define.AttackType.Magical => magicalStrengthBuffList,
                _ => throw new System.ArgumentOutOfRangeException(attackType.ToString())
            };
            var result = 1.0f;
            foreach (var (_, rate) in buffList)
            {
                result *= rate;
            }
            return result;
        }

        public float GetCutRate(Define.AttackAttribute attackAttribute)
        {
            return attackAttribute switch
            {
                Define.AttackAttribute.Slash => slashCutRateBuffList.Select(x => x.Item2).Sum(),
                Define.AttackAttribute.Blow => blowCutRateBuffList.Select(x => x.Item2).Sum(),
                Define.AttackAttribute.Thrust => thrustCutRateBuffList.Select(x => x.Item2).Sum(),
                Define.AttackAttribute.Magic => magicCutRateBuffList.Select(x => x.Item2).Sum(),
                Define.AttackAttribute.Fire => fireCutRateBuffList.Select(x => x.Item2).Sum(),
                Define.AttackAttribute.Thunder => thunderCutRateBuffList.Select(x => x.Item2).Sum(),
                _ => throw new System.ArgumentOutOfRangeException(attackAttribute.ToString())
            };
        }
    }
}
