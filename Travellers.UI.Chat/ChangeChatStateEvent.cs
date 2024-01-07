using Travellers.UI.Events;

namespace Travellers.UI.Chat;

public class ChangeChatStateEvent : UIEvent
{
	public ChatState NewState;

	public ChangeChatStateEvent(ChatState newState)
	{
		NewState = newState;
	}
}
