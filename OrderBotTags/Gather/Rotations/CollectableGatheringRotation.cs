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

    public abstract class CollectableGatheringRotation : GatheringRotation
    {
        protected AtkAddonControl MasterpieceWindow;

        public override bool ShouldForceGather
        {
            get
            {
                return Poi.Current != null && Poi.Current.Type == PoiType.Gather
                       && Poi.Current.Name.IndexOf("ephemeral", StringComparison.InvariantCultureIgnoreCase) == -1
                       && Poi.Current.Name.IndexOf("unspoiled", StringComparison.InvariantCultureIgnoreCase) == -1;
            }
        }

        protected int CurrentRarity
        {
            get
            {
                if (MasterpieceWindow != null && MasterpieceWindow.IsValid)
                {
                    return Core.Memory.Read<int>(MasterpieceWindow.Pointer + 0x000001C4);
                }

                return 0;
            }
        }

        protected bool HasDiscerningEye
        {
            get
            {
                return Core.Player.HasAura((int)AbilityAura.DiscerningEye);
            }
        }

        public override async Task<bool> Prepare(GatherCollectableTag tag)
        {
            await tag.CastAura(Ability.CollectorsGlove, AbilityAura.CollectorsGlove);

            do
            {
                while (GatheringManager.ShouldPause(DataManager.SpellCache[(uint)Ability.Preparation]))
                {
                    await Coroutine.Yield();
                }

                tag.GatherItem.GatherItem();
            }
            while ((MasterpieceWindow = await GetValidMasterPieceWindow(3000)) == null);

            return true;
        }

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            // TODO: Make dynamic rotation
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);

            await IncreaseChance(tag);

            return true;
        }

        public override async Task<bool> Gather(GatherCollectableTag tag)
        {
            int swingsRemaining;
            while ((swingsRemaining = GatheringManager.SwingsRemaining) > 0)
            {
                swingsRemaining--;
                while (!SelectYesNoItem.IsOpen && GatheringManager.SwingsRemaining > 0)
                {
                    if (MasterpieceWindow == null || !MasterpieceWindow.IsValid)
                    {
                        RaptureAtkUnitManager.Update();
                        MasterpieceWindow = await GetValidMasterPieceWindow(3000);
                    }

                    if (MasterpieceWindow != null && MasterpieceWindow.IsValid)
                    {
                        Logging.Write("Clicked Collect");
                        MasterpieceWindow.SendAction(1, 1, 0);
                    }

                    await Coroutine.Wait(3000, () => SelectYesNoItem.IsOpen);
                }

                Logging.Write("Clicked Yes");
                ff14bot.RemoteWindows.SelectYesNoItem.Yes();

                await Coroutine.Wait(5000, () => swingsRemaining == GatheringManager.SwingsRemaining);
            }

            return true;
        }

        protected override async Task<bool> IncreaseChance(GatherCollectableTag tag)
        {
            if (Core.Player.CurrentGP >= 50 && tag.GatherItem.Chance < 100)
            {
                if (Core.Player.ClassLevel >= 23 && GatheringManager.SwingsRemaining == 1)
                {
                    return await tag.Cast(Ability.IncreaseGatherChanceOnce15);
                }

                return await tag.Cast(Ability.IncreaseGatherChance5);
            }

            return false;
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

        protected async Task Discerning(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
        }

        protected async Task DiscerningMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task DiscerningImpulsive(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.ImpulsiveAppraisal);
        }

        protected async Task UtmostCaution(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.UtmostCaution);
        }

        protected async Task DiscerningUtmostMethodical(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.DiscerningEye);
            await tag.Cast(Ability.UtmostCaution);
            await tag.Cast(Ability.MethodicalAppraisal);
        }

        protected async Task UtmostImpulsive(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.UtmostCaution);
            await tag.Cast(Ability.ImpulsiveAppraisal);
        }

        protected async Task UtmostMethodical(GatherCollectableTag tag)
        {
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

        protected async Task SingleMindImpulsive(GatherCollectableTag tag)
        {
            await tag.Cast(Ability.SingleMind);
            await tag.Cast(Ability.ImpulsiveAppraisal);
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

        protected async Task AppraiseAndRebuff(GatherCollectableTag tag)
        {
            await Impulsive(tag);

            if (HasDiscerningEye)
            {
                await tag.Cast(Ability.SingleMind);
            }
            else
            {
                await tag.Cast(Ability.DiscerningEye);
            }
        }
    }
}
