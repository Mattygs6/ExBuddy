namespace ExBuddy.OrderBotTags
{
    using System;

    [Serializable]
    [Flags]
    public enum KeeperAction : byte
    {
        KeepNq = 0x01,
        KeepHq = 0x02,
        KeepAll = 0x03,
        Mooch = 0x06,
        MoochKeepNq = 0x07
    }
}
