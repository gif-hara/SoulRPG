namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct ShowMessageData
    {
        public string Message { get; }

        public string SfxName { get; }

        public ShowMessageData(string message, string sfxName)
        {
            Message = message;
            SfxName = sfxName;
        }
    }
}
