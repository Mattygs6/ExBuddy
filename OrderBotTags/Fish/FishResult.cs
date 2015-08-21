namespace ExBuddy.OrderBotTags
{
    using System;

    public class FishResult
    {
        public bool IsHighQuality { get; set; }

        public string Name { get; set; }

        public float Size { get; set; }

        public string FishName
        {
            get
            {
                if (this.IsHighQuality)
                {
                    return this.Name.Substring(0, this.Name.Length - 2);
                }

                return this.Name;
            }
        }

        public bool IsKeeper(Keeper keeper)
        {
            if (!string.Equals(keeper.Name, this.FishName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if ((!keeper.Action.HasFlag(KeeperAction.KeepHq) && this.IsHighQuality))
            {
                return false;
            }

            if ((!keeper.Action.HasFlag(KeeperAction.KeepNq) && !this.IsHighQuality))
            {
                return false;
            }

            return true;
        }

        public bool ShouldMooch(Keeper keeper)
        {
            if (!keeper.Action.HasFlag((KeeperAction)0x04))
            {
                return false;
            }

            return true;
        }
    }
}