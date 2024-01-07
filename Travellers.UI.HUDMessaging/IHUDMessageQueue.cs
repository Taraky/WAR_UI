using System.Collections.Generic;

namespace Travellers.UI.HUDMessaging;

public interface IHUDMessageQueue
{
	List<ColourParsedOSDMessage> AllMessages { get; }

	void SetMessages(List<ColourParsedOSDMessage> osdMessageList);
}
