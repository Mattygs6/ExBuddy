namespace ExBuddy.OrderBotTags.Gather
{
    using Clio.XmlEngine;

    using ExBuddy.Helpers;

    using ff14bot;
    using ff14bot.NeoProfiles;

    using TreeSharp;

    [XmlElement("StealthMoveTo")]
    public class StealthMoveToTag : MoveToTag
    {
        [XmlAttribute("UnstealthAfter")]
        public bool UnstealthAfter { get; set; }

        protected override Composite CreateBehavior()
        {
            return
                new PrioritySelector(
                    new Decorator(
                        ret => !Core.Player.HasAura((int)AbilityAura.Stealth),
                        new Action(
                            async r => await Actions.Cast(Abilities.Map[Core.Player.CurrentJob][Ability.Stealth], 250))),
                    base.CreateBehavior());
        }

        protected override async void OnDone()
        {
            base.OnDone();
            if (Core.Player.HasAura((int)AbilityAura.Stealth))
            {
                await Actions.Cast(Abilities.Map[Core.Player.CurrentJob][Ability.Stealth], 250);
            }
        }
    }
}
