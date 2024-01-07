using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public interface IHUDChatMessageInput : IHUDMessageDisplay
{
	void SetInputProcessor(ChatRoomInputProcessor inputProcessor);
}
