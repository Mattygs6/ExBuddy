namespace ExBuddy.OrderBotTags.Gather
{
    using System;

    [Flags]
    public enum CordialTime : byte
    {
        None = 0,
        BeforeGather = 1 << 0,
        AfterGather = 1 << 1,
        Auto = BeforeGather | AfterGather
    }
}
