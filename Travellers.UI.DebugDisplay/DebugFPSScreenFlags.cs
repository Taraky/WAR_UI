using Travellers.UI.HUDMessaging;

namespace Travellers.UI.DebugDisplay;

public class DebugFPSScreenFlags
{
	private static readonly char[] stringSplitter = new char[1] { ' ' };

	public bool IsEnabled;

	public bool? ShowAudio;

	public bool? ShowRam;

	public bool? ShowAdvanced;

	public DebugFPSScreenFlags(string args)
	{
		IsEnabled = true;
		if (string.IsNullOrEmpty(args.Trim()))
		{
			OSDMessage.SendMessage("FPS Screen turned on. Optional parameters are enabled or disabled using the prefix '+' or '-'.\n Adding 'off' turns the whole widget off.");
			OSDMessage.SendMessage("Params are: 'aud' for audio, 'ram' for memory, 'adv' for advanced tools and 'all' for everything");
			return;
		}
		string[] array = args.Split(stringSplitter);
		string[] array2 = array;
		foreach (string arg in array2)
		{
			CheckString(arg);
		}
	}

	private void CheckString(string arg)
	{
		if (arg == "off")
		{
			IsEnabled = false;
			return;
		}
		bool value = false;
		if (arg[0] == '+')
		{
			value = true;
		}
		else if (arg[0] != '-')
		{
			return;
		}
		arg = arg.Substring(1);
		switch (arg)
		{
		case "aud":
			ShowAudio = value;
			break;
		case "ram":
			ShowRam = value;
			break;
		case "adv":
			ShowAdvanced = value;
			break;
		case "all":
			ShowAudio = value;
			ShowRam = value;
			ShowAdvanced = value;
			break;
		}
	}
}
