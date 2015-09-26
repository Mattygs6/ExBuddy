namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	//Name, RequiredGp, RequiredTime
	[GatheringRotation("RegularNode", 0, 0)]
	public sealed class RegularNodeGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.IsEphemeral() || tag.IsUnspoiled())
			{
				return -1;
			}

			return 8000;
		}
	}
}