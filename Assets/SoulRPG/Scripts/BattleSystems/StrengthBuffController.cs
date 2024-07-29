using System.Collections.Generic;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class StrengthBuffController
    {
        private readonly List<(string, float)> physicalStrengthBuffList = new();

        private readonly List<(string, float)> magicalStrengthBuffList = new();

        public void Add(Define.AttackType attackType, string name, float rate)
        {
            switch (attackType)
            {
                case Define.AttackType.Physical:
                    physicalStrengthBuffList.Add((name, rate));
                    break;
                case Define.AttackType.Magical:
                    magicalStrengthBuffList.Add((name, rate));
                    break;
            }
        }

        public void Remove(Define.AttackType attackType, string name)
        {
            switch (attackType)
            {
                case Define.AttackType.Physical:
                    physicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
                    break;
                case Define.AttackType.Magical:
                    magicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
                    break;
            }
        }

        public float GetRate(Define.AttackType attackType)
        {
            return attackType switch
            {
                Define.AttackType.Physical => GetPhysicalStrengthRate(),
                Define.AttackType.Magical => GetMagicalStrengthRate(),
                _ => 1.0f,
            };
        }

        public float GetPhysicalStrengthRate()
        {
            float rate = 1.0f;
            foreach (var (name, buffRate) in physicalStrengthBuffList)
            {
                rate *= buffRate;
            }
            return rate;
        }

        public float GetMagicalStrengthRate()
        {
            float rate = 1.0f;
            foreach (var (name, buffRate) in magicalStrengthBuffList)
            {
                rate *= buffRate;
            }
            return rate;
        }
    }
}
