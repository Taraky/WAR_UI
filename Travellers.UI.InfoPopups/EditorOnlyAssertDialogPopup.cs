using Travellers.UI.Utility;
using WAUtilities.Assertions;

namespace Travellers.UI.InfoPopups;

public class EditorOnlyAssertDialogPopup : IDialogProvider
{
	public void ShowDialog(string title, string msg, string stackTrace)
	{
		string text = "IN EDITOR ONLY";
		text = text + "\n\nError:\n" + msg;
		text += "\n\n<color=white>Stack:";
		text += StringFormatHelper.GetStackTraceFrames(stackTrace, 4);
		text += "</color>";
		DialogPopupFacade.ShowOkDialog(title, text);
	}
}
