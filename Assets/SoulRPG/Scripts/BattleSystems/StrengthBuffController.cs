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

        public void AddPhysicalStrengthBuff(string name, float rate)
        {
            physicalStrengthBuffList.Add((name, rate));
        }

        public void AddMagicalStrengthBuff(string name, float rate)
        {
            magicalStrengthBuffList.Add((name, rate));
        }

        public void RemovePhysicalStrengthBuff(string name)
        {
            physicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
        }

        public void RemoveMagicalStrengthBuff(string name)
        {
            magicalStrengthBuffList.RemoveAll(x => x.Item1 == name);
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
