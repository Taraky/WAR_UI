using System;

namespace Travellers.UI.Exceptions;

[Serializable]
public class InvalidUIStateChangeException : Exception
{
	public InvalidUIStateChangeException(string message)
		: base(message)
	{
	}
}
