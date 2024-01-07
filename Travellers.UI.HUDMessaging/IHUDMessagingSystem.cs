using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

[InjectedInterface]
public interface IHUDMessagingSystem
{
	bool UnityDevConsoleVisibility { get; set; }

	void InitialiseForCharacter(string characterUid);

	bool IsPlayerAdmin();

	void SendChatMessage(UserInputCommand chatMessage);

	void SendDebugCommand(UserInputCommand debugCommand);

	int SubscribeToMessages(HUDMessageType messageContainerType);

	void UnsubscribeFromMessages(HUDMessageType messageContainerType, int subscriberIndex);

	void ClearLogs(HUDMessageType messageContainerType, int subscriberIndex);

	void ClearAllMessages();

	void ResetCharacter();

	List<OSDMessage> CollectMessages(HUDMessageType messageContainerType, int subscriberIndex);
}
