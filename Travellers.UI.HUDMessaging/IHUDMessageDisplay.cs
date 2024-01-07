namespace Travellers.UI.HUDMessaging;

public interface IHUDMessageDisplay
{
	void SetMessageQueue(IHUDMessageQueue messageQueue);

	void RebuildMessageView();
}
