namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static string Localized(this string self)
        {
            // TODO
            return self;
        }
        
        public static string Format(this string self, params object[] args)
        {
            return string.Format(self, args);
        }
    }
}