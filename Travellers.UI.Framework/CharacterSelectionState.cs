using System;
using System.Collections.Generic;
using Travellers.UI.Login;

namespace Travellers.UI.Framework;

public class CharacterSelectionState : WindowState
{
	private int _selectedPLayer;

	public static WindowState Default => new CharacterSelectionState(null, null, WindowLayer.CharacterLobby);

	public override WindowInteractionType InteractionType => WindowInteractionType.InteractsWithOthers;

	public CharacterSelectionState(Action onOpen, Action onClose, WindowLayer layer)
		: base(onOpen, onClose, layer)
	{
	}

	public CharacterSelectionState(Action onOpen, Action onClose, WindowLayer layer, int selectedPlayer)
		: base(onOpen, onClose, layer)
	{
		_selectedPLayer = selectedPlayer;
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
		screenLookup[typeof(CharacterSelectionScreen)] = UIObjectFactory.Create<CharacterSelectionScreen>(UIElementType.Screen, UIFillType.FillParentTransform, UIStructure.GetPriorityParent(UILayer.Bottom), isObjectActive: true);
	}

	protected override void OnEnterState()
	{
		CharacterSelectionScreen characterSelectionScreen = TryGetScreen<CharacterSelectionScreen>();
		characterSelectionScreen.SelectCharacter(_selectedPLayer);
	}
}
