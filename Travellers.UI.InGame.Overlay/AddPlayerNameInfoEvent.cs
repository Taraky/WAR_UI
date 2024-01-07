namespace Travellers.UI.InGame.Overlay;

public class AddPlayerNameInfoEvent
{
	public PlayerNameInfo PlayerNameInfo;

	public AddPlayerNameInfoEvent(PlayerNameInfo playerNameInfo)
	{
		PlayerNameInfo = playerNameInfo;
	}
}
