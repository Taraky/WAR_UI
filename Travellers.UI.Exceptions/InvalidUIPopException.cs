using System;
using Travellers.UI.Framework;

namespace Travellers.UI.Exceptions;

[Serializable]
public class InvalidUIPopException : Exception
{
	public InvalidUIPopException()
	{
	}

	public InvalidUIPopException(string windowType)
		: base(GetMessage(windowType))
	{
	}

	public InvalidUIPopException(WindowState windowType)
		: base(GetMessage(windowType.ToString()))
	{
	}

	protected static string GetMessage(string windowType)
	{
		return $"Attempting to pop a UI state that isn't in the UI stack: {windowType}";
	}
}
