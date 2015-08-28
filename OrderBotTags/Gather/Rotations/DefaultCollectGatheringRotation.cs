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

    [GatheringRotation("DefaultCollect")]
    public class DefaultCollectGatheringRotation : IGatheringRotation
    {
        protected AtkAddonControl MasterpieceWindow;

        public virtual async Task<bool> Prepare(uint slot)
        {
            // TODO: we don't want to force people to dismount to cast these, so it is eating up 3-5 seconds of gather time...
            await Actions.CastAura(Ability.CollectorsGlove, AbilityAura.CollectorsGlove);

            var hits = 0;
            GatheringItem item = null;
            while (GatheringManager.WindowOpen && hits < 2)
            {
                await
                    Coroutine.Wait(
                        5000,
                        () => (item = GatheringManager.GetGatheringItemByIndex(slot)) != null);
                if (item != null)
                {
                    item.GatherItem();
                    hits++;
                    await Coroutine.Sleep(2200);
                }
            }

            MasterpieceWindow = await GetValidMasterPieceWindow(5000);
            return true;
        }

        public virtual async Task<bool> ExecuteRotation()
        {
            await DiscerningMethodical();
            await DiscerningMethodical();
            await SingleMindMethodical();

            if (Core.Player.CurrentGP >= 50)
            {
                await Actions.Cast(Ability.IncreaseGatherChance5);
            }

            return true;
        }

        public virtual async Task<bool> Gather(uint slot)
        {
            var exCount = 0;
            while (GatheringManager.SwingsRemaining > 0)
            {
                try
                {
                    await Coroutine.Wait(5000, () => !SelectYesNoItem.IsOpen);
                    while (!SelectYesNoItem.IsOpen)
                    {
                        MasterpieceWindow.SendAction(1, 1, 0);
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

        protected async Task DiscerningMethodical()
        {
            await Actions.Cast(Ability.DiscerningEye);
            await Actions.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task SingleMindMethodical()
        {
            await Actions.Cast(Ability.SingleMind);
            await Actions.Cast(Ability.MethodicalAppraisal);
        }
    }
}