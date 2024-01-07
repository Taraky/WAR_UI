using System.Collections.Generic;

namespace Travellers.UI.Chat;

public class InputHistory
{
	private List<string> _previousEntries = new List<string>();

	private string _unfinishedString = string.Empty;

	private int _currentIndex;

	private const int _maxRemembered = 20;

	public bool IsCurrentCommand => _currentIndex == _previousEntries.Count;

	public void AddCommand(string newCommand)
	{
		_previousEntries.Add(newCommand);
		if (_previousEntries.Count >= 20)
		{
			_previousEntries.RemoveAt(0);
		}
		Reset();
	}

	public void SetUnfinishedCommand(string newCommand)
	{
		_unfinishedString = newCommand;
	}

	public string GetPreviousCommand()
	{
		if (_previousEntries.Count == 0)
		{
			return string.Empty;
		}
		if (_currentIndex <= 0)
		{
			_currentIndex = 0;
		}
		else
		{
			_currentIndex--;
		}
		return _previousEntries[_currentIndex];
	}

	public string GetNextCommand()
	{
		if (_currentIndex >= _previousEntries.Count - 1)
		{
			_currentIndex = _previousEntries.Count;
			return _unfinishedString;
		}
		_currentIndex++;
		return _previousEntries[_currentIndex];
	}

	public void Reset()
	{
		_currentIndex = _previousEntries.Count;
	}
}
