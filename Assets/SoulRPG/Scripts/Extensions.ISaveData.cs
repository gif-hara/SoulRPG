namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static void Save(this ISaveData self, string path)
        {
            SaveSystem.Save(self, path);
        }
        
        public static void Save(this ISaveData self)
        {
            SaveSystem.Save(self, self.DefaultPath);
        }
        
        public static bool Contains(this ISaveData self, string path)
        {
            return SaveSystem.Contains(path);
        }
        
        public static bool Contains(this ISaveData self)
        {
            return SaveSystem.Contains(self.DefaultPath);
        }
        
        public static void Delete(this ISaveData self, string path)
        {
            SaveSystem.Delete(path);
        }
        
        public static void Delete(this ISaveData self)
        {
            SaveSystem.Delete(self.DefaultPath);
        }
    }
}
