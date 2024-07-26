namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static class Define
    {
        public enum Direction
        {
            Up,
            Right,
            Down,
            Left,
        }

        public enum AttackAttribute
        {
            Slash,
            Blow,
            Thrust,
            Magic,
            Fire,
            Thunder,
        }

        public enum BattleResult
        {
            PlayerWin,
            PlayerLose,
        }
    }
}
