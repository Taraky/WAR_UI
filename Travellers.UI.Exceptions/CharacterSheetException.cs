using System;

namespace Travellers.UI.Exceptions;

[Serializable]
public class CharacterSheetException : Exception
{
	public CharacterSheetException(string windowType)
		: base(GetMessage(windowType))
	{
	}

	protected static string GetMessage(string windowType)
	{
		return $"Character sheet cannot be created because: {windowType}";
	}
}
