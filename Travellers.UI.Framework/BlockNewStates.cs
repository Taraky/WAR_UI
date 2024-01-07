using System;

namespace Travellers.UI.Framework;

public class BlockNewStates : WindowState
{
	public static WindowState Default => new BlockNewStates(null, null, WindowLayer.Empty);

	public override WindowInteractionType InteractionType => WindowInteractionType.InteractsWithOthers;

	public BlockNewStates(Action onOpen, Action onClose, WindowLayer layer)
		: base(onOpen, onClose, layer)
	{
	}

	public override bool IsNewStatePermitted(WindowState stateToTry)
	{
		if (stateToTry.Layer == WindowLayer.MessagePopup)
		{
			return true;
		}
		return false;
	}

	public override bool DoesButtonPressAlterState(InputButtons buttonToCheck)
	{
		return false;
	}

	public override bool ShouldPopState(WindowState stateToTry)
	{
		return false;
	}
}
