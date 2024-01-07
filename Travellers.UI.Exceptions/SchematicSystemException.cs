using System;

namespace Travellers.UI.Exceptions;

[Serializable]
public class SchematicSystemException : Exception
{
	public SchematicSystemException(string message)
		: base(message)
	{
	}
}
