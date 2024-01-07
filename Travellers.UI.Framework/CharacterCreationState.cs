using System;
using System.Collections.Generic;
using Travellers.UI.Login;

namespace Travellers.UI.Framework;

public class CharacterCreationState : WindowState
{
	public static WindowState Default => new CharacterCreationState(null, null, WindowLayer.CharacterLobby);

	public override WindowInteractionType InteractionType => WindowInteractionType.InteractsWithOthers;

	public CharacterCreationState(Action onOpen, Action onClose, WindowLayer layer)
		: base(onOpen, onClose, layer)
	{
	}

	public override bool DoesButtonPressAlterState(InputButtons buttonToCheck)
	{
		return true;
	}

	public override bool ShouldPopState(WindowState stateToTry)
	{
		return stateToTry.Layer == Layer;
	}

	protected override void CreateScreens(Dictionary<Type, UIScreen> screenLookup)
	{
		screenLookup[typeof(CharacterCreationScreen)] = UIObjectFactory.Create<CharacterCreationScreen>(UIElementType.Screen, UIFillType.FillParentTransform, UIStructure.GetPriorityParent(UILayer.Bottom), isObjectActive: true);
	}
}
