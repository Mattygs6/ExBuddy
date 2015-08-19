namespace ExBuddy.OrderBotTags
{
    using System;

    [Serializable]
    [Flags]
    public enum KeeperAction : byte
    {
        KeepNq = 0x01,
        KeepHq = 0x02,
        KeepAll = 0x03, // KeepNq | KeepHq
        Mooch = 0x06, // KeepHq | 0x04
        MoochKeepNq = 0x07 // KeepNq | KeepHq | 0x04
    }
}
