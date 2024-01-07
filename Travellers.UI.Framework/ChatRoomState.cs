using System;
using Travellers.UI.HUDMessaging;

namespace Travellers.UI.Framework;

public class ChatRoomState : WindowState
{
	public static WindowState Default => new ChatRoomState(delegate
	{
		PlayerMessagingScreen.MakeChatRoomVisible(show: true);
	}, delegate
	{
		PlayerMessagingScreen.MakeChatRoomVisible(show: false);
	}, WindowLayer.Chat);

	public override WindowInteractionType InteractionType => WindowInteractionType.InteractsWithOthers;

	public ChatRoomState(Action onOpen, Action onClose, WindowLayer layer)
		: base(onOpen, onClose, layer)
	{
	}

	public override bool DoesButtonPressAlterState(InputButtons buttonToCheck)
	{
		switch (buttonToCheck)
		{
		case InputButtons.Cancel:
		case InputButtons.OpenChat:
			UIWindowController.PopState(this);
			return true;
		case InputButtons.ToggleHUDDebugDisplay:
			UIWindowController.PopState(this);
			UIWindowController.PushState(DebugCommandPopupUIState.Default);
			return true;
		default:
			return true;
		}
	}

	public override bool ShouldPopState(WindowState stateToTry)
	{
		return false;
	}
}
