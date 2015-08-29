namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;

    using ff14bot;
    using ff14bot.Managers;

    [GatheringRotation("Collect550", 600, 34)]
    public class Collect550GatheringRotation : DefaultCollectGatheringRotation
    {
        public override bool ForceGatherIfMissingGpOrTime
        {
            get
            {
                return true;
            }
        }

        public override async Task<bool> ExecuteRotation(GatherCollectable tag)
        {
            if (tag.GatherItem.Level < 60)
            {
                await Actions.Cast(Ability.ImpulsiveAppraisal);
                await Actions.Cast(Ability.ImpulsiveAppraisal);
                await Actions.Cast(Ability.MethodicalAppraisal);

                if (tag.GatherItem.Level >= 58 && (Core.Player.CurrentGP >= 650 || (Core.Player.MaxGP - Core.Player.CurrentGP) <= 50))
                {
                    await Actions.Cast(Ability.IncreaseGatherChance5);
                }

                return true;
            }

            var appraisalsRemaining = 4;
            await Actions.Cast(Ability.ImpulsiveAppraisal);
            appraisalsRemaining--;

            if (HasDiscerningEye)
            {
                await SingleMindUtmostMethodical();
                appraisalsRemaining--;
            }

            await Actions.Cast(Ability.ImpulsiveAppraisal);
            appraisalsRemaining--;

            if (HasDiscerningEye)
            {
                await SingleMindUtmostMethodical();
                appraisalsRemaining--;
            }

            if (appraisalsRemaining == 2)
            {
                await Actions.Cast(Ability.MethodicalAppraisal);

                return true;
            }

            if (appraisalsRemaining == 1)
            {
                await Actions.Cast(Ability.DiscerningEye);
                await Actions.Cast(Ability.UtmostCaution);
                await Actions.Cast(Ability.MethodicalAppraisal);
            }

            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }
    }
}