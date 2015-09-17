using System;

namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    using System.Threading.Tasks;
    using ff14bot;
    using ff14bot.Helpers;
    using ff14bot.Managers;

    [GatheringRotation("NewbCollect", 600, 24)]
    public sealed class NewbCollectGatheringRotation : CollectableGatheringRotation, IGetOverridePriority
    {

        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            var rarity = 0;
            if (tag.CollectableItem.PlusPlus == 0)
            {
                await DiscerningMethodical(tag);
                Logging.Write("Post non-plus Rarity: " + (rarity = CurrentRarity));
            }
            var level = Core.Player.ClassLevel;

            if (rarity >= 119 && rarity <= 124)
            {
                if (level >= 53)
                {
                    await CallRotation("Try Harder", () => TryHarder(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (rarity >= 125 && rarity <= 134)
            {
                if (level >= 51)
                {
                    await CallRotation("Try Hard", () => TryHard(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (rarity >= 135 && rarity <= 137)
            {
                if (level >= 53)
                {
                    await CallRotation("Get One", () => GetOne(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (rarity >= 138 && rarity <= 140)
            {
                if (level >= 57)
                {
                    await CallRotation("Get One+", () => GetOnePlus(tag));
                }

                if (level >= 53)
                {
                    await CallRotation("Get One", () => GetOne(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (rarity >= 141 && rarity <= 149)
            {
                if (level >= 57)
                {
                    await CallRotation("Get One Alternate", () => GetOnePlusPlusAlternate(tag));
                }

                if (level >= 53)
                {
                    await CallRotation("Get One", () => GetOne(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (tag.CollectableItem.PlusPlus == 1)
            {
                await CallRotation("Get One++", () => GetOnePlusPlus(tag));
                return true;
            }

            if (rarity >= 150 && rarity <= 155)
            {
                if (level >= 50)
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (rarity >= 156 && rarity <= 160)
            {
                if (level >= 57)
                {
                    await CallRotation("Get Two+", () => GetTwoPlus(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }


            if (rarity >= 161 && rarity <= 168)
            {
                if (level >= 57)
                {
                    await CallRotation("Get Two Alternate", () => GetTwoPlusPlusAlternate(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }

            if (tag.CollectableItem.PlusPlus == 2)
            {
                {
                    await CallRotation("Get Two++", () => GetTwoPlusPlus(tag));
                }
                return true;
            }


            if (rarity >= 169)
            {
                if (level >= 57)
                {
                    await CallRotation("Get Three", () => GetThree(tag));
                }

                else
                {
                    await CallRotation("Get Two", () => GetTwo(tag));
                }
                return true;
            }



            return false;
        }

        private static async Task CallRotation(string rotationName, Func<Task<bool>> callBack)
        {
            Logging.Write("Using Rotation: " + rotationName);
            await callBack();
            Logging.Write("Exiting Rotation: " + rotationName);
        }

        public async Task<bool> TryHard(GatherCollectableTag tag)
        {
            //Try Hard - Level 53 Minimum           
            await UtmostImpulsive(tag);

            if (HasDiscerningEye)
            {
                Logging.Write("Discerning Eye Proc!");
                await UtmostMethodical(tag);
                await DiscerningMethodical(tag);
                await IncreaseChance(tag);
            }

            else
            {
                Logging.Write("No Discerning Eye Proc!");
                await DiscerningImpulsive(tag);
                await UtmostMethodical(tag);
                await IncreaseChance(tag);
            }
            return true;
        }

        public async Task<bool> TryHarder(GatherCollectableTag tag)
        {
            //Try Harder - Level 53 Minimum            
            await DiscerningImpulsive(tag);
            await UtmostImpulsive(tag);
            await UtmostMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetOne(GatherCollectableTag tag)
        {
            //Get One - Level 53 Minimum           
            await DiscerningMethodical(tag);
            await UtmostMethodical(tag);
            await UtmostMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetOnePlus(GatherCollectableTag tag)
        {
            //Get One+ - Level 57 Minimum
            await UtmostCaution(tag);
            await AppraiseAndRebuff(tag);
            await Methodical(tag);
            await UtmostMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetOnePlusPlusAlternate(GatherCollectableTag tag)
        {
            //Get One++ Alternative - Level 53 Minimum
            Logging.Write("Hey! Listen! You can update this item to use Get One++!!! Using Rotation: Get One for now... :'(");
            await DiscerningMethodical(tag);
            await UtmostMethodical(tag);
            await UtmostMethodical(tag);
            await IncreaseChance(tag);
            return true;

        }

        public async Task<bool> GetOnePlusPlus(GatherCollectableTag tag)
        {
            //Get One++ - Level 57 Minimum
            await UtmostCaution(tag);
            await AppraiseAndRebuff(tag);
            await Methodical(tag);
            await UtmostCaution(tag);
            await AppraiseAndRebuff(tag);
            await Methodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetTwo(GatherCollectableTag tag)
        {
            //Get Two - Level 50 Minimum
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetTwoPlus(GatherCollectableTag tag)
        {
            //Get Two+ - Level 53 Minimum
            await Discerning(tag);
            await AppraiseAndRebuff(tag);
            await Methodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetTwoPlusPlusAlternate(GatherCollectableTag tag)
        {
            //Get Two++ Alternative - Level 50 Minimum
            Logging.Write("Hey! Listen! You can update this item to use Get Two++!!! Using Rotation: Get Two for now... :'(");
            await DiscerningMethodical(tag);
            await DiscerningMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        public async Task<bool> GetTwoPlusPlus(GatherCollectableTag tag)
        {
            //Get Two++ - Level 57 Minimum
            {
                await Discerning(tag);
                await AppraiseAndRebuff(tag);
                await AppraiseAndRebuff(tag);
                await Methodical(tag);
                await IncreaseChance(tag);
                return true;
            }
        }

        public async Task<bool> GetThree(GatherCollectableTag tag)
        {
            //Get Three - Level 57 Minimum
            await SingleMindMethodical(tag);
            await DiscerningMethodical(tag);
            await IncreaseChance(tag);
            return true;
        }

        int IGetOverridePriority.GetOverridePriority(GatherCollectableTag tag)
        {
            if (tag.IsUnspoiled())
            {
                // We need 5 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 5)
                {
                    return -1;
                }
            }

            if (tag.IsEphemeral())
            {
                // We need 4 swings to use this rotation
                if (GatheringManager.SwingsRemaining < 4)
                {
                    return -1;
                }
            }

            // if we have a collectable Priority 0
            if (tag.CollectableItem != null && tag.CollectableItem.Value == 0)
            {
                return 80;
            }

            return -1;
        }
    }
}