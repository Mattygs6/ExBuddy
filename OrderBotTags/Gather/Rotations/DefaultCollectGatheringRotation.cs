namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ff14bot;
    using ff14bot.Helpers;
    using ff14bot.Managers;
    using ff14bot.RemoteWindows;
    
    // Gathers approx 516 if perception capped.
    [GatheringRotation("DefaultCollect")]
    public class DefaultCollectGatheringRotation : IGatheringRotation
    {
        protected AtkAddonControl MasterpieceWindow;

        protected bool HasDiscerningEye
        {
            get
            {
                return Core.Player.HasAura((int)AbilityAura.DiscerningEye);
            }
        }

        public virtual bool CanOverride
        {
            get
            {
                return true;
            }
        }

        public virtual bool ForceGatherIfMissingGpOrTime
        {
            get
            {
                return false;
            }
        }

        public virtual async Task<bool> Prepare(GatherCollectableTag tag)
        {
            // TODO: we don't want to force people to dismount to cast these, so it is eating up 3-5 seconds of gather time...
            await tag.CastAura(Ability.CollectorsGlove, AbilityAura.CollectorsGlove);

            var hits = 0;
            while (GatheringManager.WindowOpen && hits < 2)
            {
                hits += tag.GatherItem.GatherItem() ? 1 : 0;
                await Coroutine.Sleep(1000);
            }

            MasterpieceWindow = await GetValidMasterPieceWindow(5000);

            return true;
        }

        public virtual async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);

            await IncreaseChance(tag);

            return true;
        }

        public virtual async Task<bool> Gather(GatherCollectableTag tag)
        {
            var exCount = 0;
            while (GatheringManager.SwingsRemaining > 0)
            {
                try
                {
                    await Coroutine.Wait(5000, () => !SelectYesNoItem.IsOpen);
                    while (!SelectYesNoItem.IsOpen)
                    {
                        if (MasterpieceWindow == null || !MasterpieceWindow.IsValid)
                        {
                            RaptureAtkUnitManager.Update();
                            MasterpieceWindow = await GetValidMasterPieceWindow(5000);
                        }

                        if (MasterpieceWindow != null && MasterpieceWindow.IsValid)
                        {
                            MasterpieceWindow.SendAction(1, 1, 0);    
                        }
                        
                        await Coroutine.Wait(1000, () => SelectYesNoItem.IsOpen);
                    }

                    ff14bot.RemoteWindows.SelectYesNoItem.Yes();
                    await Coroutine.Sleep(2200);
                }
                catch (Exception ex)
                {
                    if (++exCount < 3)
                    {
                        Logging.Write("An Exception has occured, but is not yet fatal to our task. Count: " + exCount);
                        Logging.WriteException(ex);
                        Logging.Write("Attempting to continue");
                    }
                    else
                    {
                        TreeRoot.Stop("Too many exceptions");
                        throw;
                    }
                }

                await Coroutine.Wait(300, () => GatheringManager.SwingsRemaining == 0);
            }

            return true;
        }

        public virtual int ShouldOverrideSelectedGatheringRotation(GatherCollectableTag tag)
        {
            if (tag.Node.EnglishName.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                // We need 5 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 5)
                {
                    return -1;
                }
            }

            if (tag.Node.EnglishName.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                // We need 4 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 4)
                {
                    return -1;
                }
            }

            // if we have a collectable && the collectable value is greater than or equal to 516: Priority 516
            if (tag.CollectableItem != null && tag.CollectableItem.Value >= 516)
            {
                return 516;
            }

            return -1;
        }

        protected virtual async Task<AtkAddonControl> GetValidMasterPieceWindow(int timeoutMs)
        {
            AtkAddonControl atkControl = null;
            await
                Coroutine.Wait(
                    timeoutMs,
                    () =>
                    (atkControl =
                     RaptureAtkUnitManager.Controls.FirstOrDefault(c => c.Name == "GatheringMasterpiece" && c.IsValid))
                    != null);

            return atkControl;
        }

        protected virtual async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return false;
        }

        protected async Task DiscerningMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task DiscerningUtmostMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.UtmostCaution);
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task Impulsive(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.ImpulsiveAppraisal);
        }

        protected async Task Methodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task SingleMindMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.SingleMind);
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task SingleMindUtmostMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.SingleMind);
            await tag.Cast(Ability.UtmostCaution);
            await tag.Cast(Ability.MethodicalAppraisal);
        }
    }
}