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
        private readonly List<(string id, float rate)> physicalStrengthBuffList = new();

        private readonly List<(string id, float rate)> magicalStrengthBuffList = new();

        private readonly List<(string id, float rate)> slashCutRateBuffList = new();

        private readonly List<(string id, float rate)> blowCutRateBuffList = new();

        private readonly List<(string id, float rate)> thrustCutRateBuffList = new();

        private readonly List<(string id, float rate)> magicCutRateBuffList = new();

        private readonly List<(string id, float rate)> fireCutRateBuffList = new();

        private readonly List<(string id, float rate)> thunderCutRateBuffList = new();

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
            if (buffList.Any(x => x.id == name))
            {
                return;
            }
            buffList.Add((name, rate));
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
                Define.AttackAttribute.Slash => slashCutRateBuffList.Select(x => x.rate).Sum(),
                Define.AttackAttribute.Blow => blowCutRateBuffList.Select(x => x.rate).Sum(),
                Define.AttackAttribute.Thrust => thrustCutRateBuffList.Select(x => x.rate).Sum(),
                Define.AttackAttribute.Magic => magicCutRateBuffList.Select(x => x.rate).Sum(),
                Define.AttackAttribute.Fire => fireCutRateBuffList.Select(x => x.rate).Sum(),
                Define.AttackAttribute.Thunder => thunderCutRateBuffList.Select(x => x.rate).Sum(),
                _ => throw new System.ArgumentOutOfRangeException(attackAttribute.ToString())
            };
        }
    }
}
