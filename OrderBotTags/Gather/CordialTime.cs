namespace ExBuddy.OrderBotTags.Gather
{
    using System;

    [Flags]
    public enum CordialTime : byte
    {
        None = 0,
        BeforeGather = 1 << 1,
        AfterGather = 1 << 2,
        Auto = BeforeGather | AfterGather
    }
}
