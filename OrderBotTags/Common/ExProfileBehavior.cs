namespace ExBuddy.OrderBotTags.Common
{
    using ff14bot.Managers;
    using ff14bot.NeoProfiles;
    using ff14bot.Objects;

    public abstract class ExProfileBehavior : ProfileBehavior
    {
        protected static LocalPlayer Me
        {
            get
            {
                return GameObjectManager.LocalPlayer;
            }
        }
    }
}
