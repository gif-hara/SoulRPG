namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CharacterGrowthParameter
    {
        private int vitality;
        public int Vitality => vitality;

        public int stamina;
        public int Stamina => stamina;

        private int physicalStrength;
        public int PhysicalStrength => physicalStrength;

        private int magicalStrength;
        public int MagicalStrength => magicalStrength;

        private int speed;
        public int Speed => speed;

        public void AddVitality(int value)
        {
            vitality += value;
        }

        public void AddStamina(int value)
        {
            stamina += value;
        }

        public void AddPhysicalStrength(int value)
        {
            physicalStrength += value;
        }

        public void AddMagicalStrength(int value)
        {
            magicalStrength += value;
        }

        public void AddSpeed(int value)
        {
            speed += value;
        }
    }
}
