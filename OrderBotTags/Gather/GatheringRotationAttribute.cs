namespace ExBuddy.OrderBotTags.Gather
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GatheringRotationAttribute : Attribute
    {
        public GatheringRotationAttribute(string name)
            : this(name, 0, 0)
        {
        }

        public GatheringRotationAttribute(string name, ushort requiredGp, byte requiredTimeInSeconds)
        {
            this.Name = name;
            this.RequiredGp = requiredGp;
            this.RequiredTimeInSeconds = requiredTimeInSeconds;
        }

        public string Name { get; private set; }
        public ushort RequiredGp { get; set; }
        public byte RequiredTimeInSeconds { get; set; }
    }
}