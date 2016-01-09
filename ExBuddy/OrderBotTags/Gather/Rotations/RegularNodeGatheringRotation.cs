namespace ExBuddy.OrderBotTags.Gather.Rotations
{
	using ExBuddy.Attributes;
	using ExBuddy.Interfaces;

	//Name, RequiredTime, RequiredGpBreakpoints
	[GatheringRotation("RegularNode")]
	public sealed class RegularNodeGatheringRotation : GatheringRotation, IGetOverridePriority
	{
		#region IGetOverridePriority Members

		int IGetOverridePriority.GetOverridePriority(ExGatherTag tag)
		{
			if (tag.IsEphemeral() || tag.IsUnspoiled() || tag.CollectableItem != null)
			{
				return -1;
			}

			return 8000;
		}

		#endregion
	}
}