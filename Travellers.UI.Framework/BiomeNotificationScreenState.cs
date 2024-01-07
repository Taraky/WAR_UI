using System;
using System.Collections.Generic;
using Bossa.Travellers.Biomes;
using UnityEngine;

namespace Travellers.UI.Framework;

public class BiomeNotificationScreenState : WindowState
{
	private readonly BiomeType _biomeType;

	private readonly Vector3 _pos;

	public override WindowInteractionType InteractionType => WindowInteractionType.Dissociated;

	private BiomeNotificationScreenState(Action onOpen, Action onClose, WindowLayer layer)
		: base(onOpen, onClose, layer)
	{
	}

	public BiomeNotificationScreenState(BiomeType biomeType, Vector3 pos)
		: this(biomeType, pos, WindowLayer.BiomeNotification)
	{
	}

	public BiomeNotificationScreenState(BiomeType biomeType, Vector3 pos, WindowLayer layer)
		: this(null, null, layer)
	{
		_biomeType = biomeType;
		_pos = pos;
	}

	public override bool DoesButtonPressAlterState(InputButtons buttonToCheck)
	{
		switch (buttonToCheck)
		{
		case InputButtons.Cancel:
			UIWindowController.PushState(InGameMenuUIState.Default);
			return true;
		case InputButtons.OpenInventory:
			UIWindowController.PushState(MainInventoryUIState.LastOpened);
			return true;
		case InputButtons.OpenSocial:
			UIWindowController.PushState(SocialScreenUIState.Default);
			return true;
		case InputButtons.OpenChat:
			UIWindowController.PushState(ChatRoomState.Default);
			return true;
		case InputButtons.OpenDebugConsole:
			UIWindowController.PushState(DebugCommandPopupUIState.Default);
			return true;
		case InputButtons.OpenFeedback:
			UIWindowController.PushState(FeedbackPopupUIState.Default);
			return true;
		case InputButtons.Emotes:
			UIWindowController.PushState(EmoteScreenState.Default);
			return true;
		default:
			return true;
		}
	}

	public override bool ShouldPopState(WindowState stateToTry)
	{
		return stateToTry.Layer == Layer;
	}

	protected override void CreateScreens(Dictionary<Type, UIScreen> screenLookup)
	{
		screenLookup[typeof(BiomeNotificationScreen)] = UIObjectFactory.Create<BiomeNotificationScreen>(UIElementType.Screen, UIFillType.KeepOriginalAnchoringAndPosition, UIStructure.GetPriorityParent(UILayer.Top), isObjectActive: true);
	}

	protected override void OnEnterState()
	{
		BiomeNotificationScreen biomeNotificationScreen = TryGetScreen<BiomeNotificationScreen>();
		biomeNotificationScreen.SetData(_biomeType, _pos, this);
	}
}
