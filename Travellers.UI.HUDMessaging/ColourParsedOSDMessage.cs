using System;

namespace Travellers.UI.HUDMessaging;

public class ColourParsedOSDMessage
{
	public string ColourParsedMessage;

	public OSDMessage InnerMessage { get; private set; }

	public ColourParsedOSDMessage(OSDMessage innerMessage, Func<OSDMessage, string> parseFunc)
	{
		InnerMessage = innerMessage;
		ColourParsedMessage = parseFunc(innerMessage);
	}
}
