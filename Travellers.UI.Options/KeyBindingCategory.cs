using System;

namespace Travellers.UI.Options;

[Serializable]
public struct KeyBindingCategory
{
	public string name;

	public InputButtons[] buttons;

	public KeyBindingCategory(string name, InputButtons[] buttons)
	{
		this.name = name;
		this.buttons = buttons;
	}
}
