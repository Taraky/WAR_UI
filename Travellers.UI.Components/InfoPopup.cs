using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Components;

public class InfoPopup : OldUIScreenBase
{
	[SerializeField]
	private Text actionLabel;

	[SerializeField]
	private Text keyPress;

	[SerializeField]
	private Text instructionAction;

	[SerializeField]
	private GameObject button;

	public void DisplayMessage(string action, string key, bool hold)
	{
		base.gameObject.SetActive(value: true);
		button.SetActive(value: true);
		actionLabel.text = action;
		keyPress.text = key;
		instructionAction.text = ((!hold) ? "Press" : "Hold");
	}

	public void DisplayUseToolMessage(string tool, string action)
	{
		base.gameObject.SetActive(value: true);
		button.SetActive(value: false);
		actionLabel.text = action;
		keyPress.text = string.Empty;
		instructionAction.text = "Use " + tool;
	}
}
