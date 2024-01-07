using Bossa.Travellers.World;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ChatMessageQueueController : MessageQueueController
{
	private readonly IHUDChatMessageInput _chatMessageInput;

	private readonly ChatFilterModule _chatFilterModule;

	private readonly ChatRoomInputProcessor _inputProcessor;

	public ChatMessageQueueController(IHUDChatMessageInput chatMessageInput, ChatFilterModule chatFilterModule, IHUDMessageQueue messageQueue, HUDMessageType messageSystemType, IHUDMessagingSystem messagingSystem, params MessageType[] typesToInclude)
		: base(chatMessageInput, messageQueue, messageSystemType, messagingSystem, typesToInclude)
	{
		_chatMessageInput = chatMessageInput;
		_chatFilterModule = chatFilterModule;
		_inputProcessor = new ChatRoomInputProcessor(_chatFilterModule);
	}

	public void SetChatActive(bool isActive)
	{
		_isActive = isActive;
		if (_isActive)
		{
			SetMessageQueue();
			SetInputProcessor();
		}
	}

	private void SetInputProcessor()
	{
		_chatMessageInput.SetInputProcessor(_inputProcessor);
	}
}
