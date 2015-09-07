using System.Threading.Tasks;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;

namespace ExBuddy.OrderBotTags.Gather.Rotations
{
    [GatheringRotation("NewbCollect", 0, 24)]
    public sealed class Collect115GatheringRotation : CollectableGatheringRotation, IGetOverridePriority
    {
        public override async Task<bool> ExecuteRotation(GatherCollectableTag tag)
        {
            Logging.Write("Pre-Rotation Rarity: " + CurrentRarity);
            if (tag.CollectableItem.PlusPlus == 0)
            {
                await DiscerningMethodical(tag);
                Logging.Write("Post non-plus Rarity: " + CurrentRarity);
            }

            if (CurrentRarity >= 119 && CurrentRarity <= 124)
            {
                //Try Harder
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Try Harder");
                await DiscerningImpulsive(tag);
                await UtmostImpulsive(tag);
                await UtmostMethodical(tag);
                Logging.Write("Exiting Rotation: Try Harder");
                return true;
            }

            if (CurrentRarity >= 125 && CurrentRarity <= 134)
            {
                //Try Hard
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Try Hard");
                await UtmostImpulsive(tag);

                if (HasDiscerningEye)
                {
                    Logging.Write("Discerning Eye Proc!");
                    await UtmostMethodical(tag);
                    await DiscerningMethodical(tag);
                }

                else
                {
                    Logging.Write("No Discerning Eye Proc!");
                    await DiscerningImpulsive(tag);
                    await UtmostMethodical(tag);
                }
                Logging.Write("Exiting Rotation: Try Hard");
                return true;
            }

            if (CurrentRarity >= 135 && CurrentRarity <= 137)
            {
                //Get One
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get One");
                await DiscerningMethodical(tag);
                await UtmostMethodical(tag);
                await UtmostMethodical(tag);
                Logging.Write("Exiting Rotation: Get One");
                return true;
            }

            if (CurrentRarity >= 138 && CurrentRarity <= 140)
            {
                //Get One+
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get One+");
                await UtmostImpulsive(tag);

                if (HasDiscerningEye)
                {
                    Logging.Write("Discerning Eye Proc!");
                    await SingleMindMethodical(tag);
                    await UtmostMethodical(tag);
                }

                else
                {
                    Logging.Write("No Discerning Eye Proc!");
                    await DiscerningMethodical(tag);
                    await UtmostMethodical(tag);
                }
                Logging.Write("Exiting Rotation: Get One+");
                return true;
            }

            if (CurrentRarity >= 141 && CurrentRarity <= 149)
            {
                //Get One++
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write(
                    "Hey! Listen! You can update this item to use Get One++!!! Using Rotation: Get One+ for now... :'(");
                await UtmostImpulsive(tag);

                if (HasDiscerningEye)
                {
                    await SingleMindMethodical(tag);
                    await UtmostImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }

                else
                {
                    await DiscerningMethodical(tag);
                    await UtmostImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }
                Logging.Write("Exiting Rotation: GetOne+ - Alternate");
                return true;
            }

            if (tag.CollectableItem.PlusPlus == 1)
            {
                //Get One++
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get One++");
                await UtmostImpulsive(tag);
                if (HasDiscerningEye)
                {
                    await SingleMindMethodical(tag);
                    await UtmostImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }

                else
                {
                    await DiscerningMethodical(tag);
                    await UtmostImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }
                Logging.Write("Exiting Rotation: Get One++");
                return true;
            }

            if (CurrentRarity >= 150 && CurrentRarity <= 155)
            {
                //Get Two
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get Two");
                await DiscerningMethodical(tag);
                await DiscerningMethodical(tag);
                Logging.Write("Exiting Rotation:  Get Two");
                return true;
            }

            if (CurrentRarity >= 156 && CurrentRarity <= 160)
            {
                //Get Two +
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get Two+");
                await DiscerningImpulsive(tag);

                if (HasDiscerningEye)
                {
                    Logging.Write("Discerning Eye Proc!");
                    await SingleMindMethodical(tag);
                }

                else
                {
                    Logging.Write("No Discerning Eye Proc!");
                    await DiscerningMethodical(tag);
                }
                Logging.Write("Exiting Rotation: Get Two+");
                return true;
            }

            if (CurrentRarity >= 161 && CurrentRarity <= 168)
            {
                //Get Two++
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write(
                    "Hey! Listen! You can update this item to use Get Two++!!! Using Rotation: Get Two+ for now... :'(");
                await DiscerningImpulsive(tag);

                if (HasDiscerningEye)
                {
                    Logging.Write("Discerning Eye Proc!");
                    await SingleMindImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }
                else
                {
                    Logging.Write("No Discerning Eye Proc!");
                    await DiscerningImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }
                Logging.Write("Exiting Rotation: Get Two++ - Alternate");
                return true;
            }

            if (tag.CollectableItem.PlusPlus == 2)
            {
                //Get Two++
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get Two++");
                await DiscerningImpulsive(tag);

                if (HasDiscerningEye)
                {
                    Logging.Write("Discerning Eye Proc!");
                    await SingleMindImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }

                else
                {
                    Logging.Write("No Discerning Eye Proc!");
                    await DiscerningImpulsive(tag);

                    if (HasDiscerningEye)
                    {
                        Logging.Write("Discerning Eye Proc!");
                        await SingleMindMethodical(tag);
                    }

                    else
                    {
                        Logging.Write("No Discerning Eye Proc!");
                        await DiscerningMethodical(tag);
                    }
                }
                Logging.Write("Exiting Rotation: Get Two++");
                return true;
            }

            if (CurrentRarity >= 169)
            {
                //Get Three
                Logging.Write("Rarity: " + CurrentRarity);
                Logging.Write("Using Rotation: Get Three");
                await SingleMindMethodical(tag);
                await DiscerningMethodical(tag);
                Logging.Write("Exiting Rotation: Get Three");
                return true;
            }
            return false;
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