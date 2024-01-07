using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI;

[InjectedReferenceDataObject]
public class UIColourAndTextReferenceData : UISystem
{
	public ColourReference UIColours;

	public TextStyleReference UITextStyles;

	public InputFieldStyleReference UIInputConstraintStyle;

	public UIColourAndTextReferenceData()
	{
		UIColours = Resources.Load("UI/ReferenceData/UIColourReference") as ColourReference;
		UITextStyles = Resources.Load("UI/ReferenceData/UITextStyleReference") as TextStyleReference;
		UIInputConstraintStyle = Resources.Load("UI/ReferenceData/UIInputFieldStyleReference") as InputFieldStyleReference;
	}

	protected override void AddListeners()
	{
	}

	public override void Init()
	{
	}

	public override void ControlledUpdate()
	{
	}

	protected override void Dispose()
	{
	}
}
