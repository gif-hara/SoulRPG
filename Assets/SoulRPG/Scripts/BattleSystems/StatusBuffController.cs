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
            switch (statusType)
            {
                case Define.StatusType.PhysicalStrength:
                    if (physicalStrengthBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    physicalStrengthBuffList.Add((name, rate));
                    break;
                case Define.StatusType.MagicalStrength:
                    if (magicalStrengthBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    magicalStrengthBuffList.Add((name, rate));
                    break;
                case Define.StatusType.SlashCutRate:
                    if (slashCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    slashCutRateBuffList.Add((name, rate));
                    break;
                case Define.StatusType.BlowCutRate:
                    if (blowCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    blowCutRateBuffList.Add((name, rate));
                    break;
                case Define.StatusType.ThrustCutRate:
                    if (thrustCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    thrustCutRateBuffList.Add((name, rate));
                    break;
                case Define.StatusType.MagicCutRate:
                    if (magicCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    magicCutRateBuffList.Add((name, rate));
                    break;
                case Define.StatusType.FireCutRate:
                    if (fireCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    fireCutRateBuffList.Add((name, rate));
                    break;
                case Define.StatusType.ThunderCutRate:
                    if (thunderCutRateBuffList.Any(x => x.Item1 == name))
                    {
                        return;
                    }
                    thunderCutRateBuffList.Add((name, rate));
                    break;
            }
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
            switch (attackType)
            {
                case Define.AttackType.Physical:
                    return 1.0f + physicalStrengthBuffList.Select(x => x.Item2).Sum();
                case Define.AttackType.Magical:
                    return 1.0f + magicalStrengthBuffList.Select(x => x.Item2).Sum();
                default:
                    Assert.IsTrue(false, $"攻撃タイプが不正です {attackType}");
                    return 1.0f;
            }
        }

        public float GetCutRate(Define.AttackAttribute attackAttribute)
        {
            switch (attackAttribute)
            {
                case Define.AttackAttribute.Slash:
                    return slashCutRateBuffList.Select(x => x.Item2).Sum();
                case Define.AttackAttribute.Blow:
                    return blowCutRateBuffList.Select(x => x.Item2).Sum();
                case Define.AttackAttribute.Thrust:
                    return thrustCutRateBuffList.Select(x => x.Item2).Sum();
                case Define.AttackAttribute.Magic:
                    return magicCutRateBuffList.Select(x => x.Item2).Sum();
                case Define.AttackAttribute.Fire:
                    return fireCutRateBuffList.Select(x => x.Item2).Sum();
                case Define.AttackAttribute.Thunder:
                    return thunderCutRateBuffList.Select(x => x.Item2).Sum();
                default:
                    Assert.IsTrue(false, $"攻撃属性が不正です {attackAttribute}");
                    return 0.0f;
            }
        }
    }
}
