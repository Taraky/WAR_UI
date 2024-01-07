using System.Collections.Generic;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.Options;
using UnityEngine;

namespace Travellers.UI.InfoPopups;

public class MusicalInstrumentControlsScreen : UIScreen
{
	[SerializeField]
	private TextMeshProUGUI _leftClickTextMesh;

	[SerializeField]
	private TextMeshProUGUI _helpTextMesh;

	[SerializeField]
	private TextMeshProUGUI _leftClickControlTextMesh;

	[SerializeField]
	private TextMeshProUGUI _helpControlTextMesh;

	[SerializeField]
	private TextMeshProUGUI _eTextMesh;

	[SerializeField]
	private TextMeshProUGUI _fTextMesh;

	[SerializeField]
	private TextMeshProUGUI _gTextMesh;

	[SerializeField]
	private TextMeshProUGUI _aTextMesh;

	[SerializeField]
	private TextMeshProUGUI _bTextMesh;

	[SerializeField]
	private TextMeshProUGUI _cTextMesh;

	[SerializeField]
	private TextMeshProUGUI _dTextMesh;

	[SerializeField]
	private TextMeshProUGUI _eOctaveTextMesh;

	[SerializeField]
	private TextMeshProUGUI _modifier1TextMesh;

	[SerializeField]
	private TextMeshProUGUI _modifier2TextMesh;

	[SerializeField]
	private GameObject _controlsMessage;

	[SerializeField]
	private GameObject _controlBindings;

	private KeyBindingsLibrarian _keyBindingsLibrarian;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAInGameEvents.UpdatePlayingMusicalInstrumentState, OnUpdatePlayingMusicalInstrument);
	}

	public static void ChangeControlState(MusicalInstrumentScreenState state)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAInGameEvents.UpdatePlayingMusicalInstrumentState, new UpdatePlayingMusicalInstrumentEvent(state));
	}

	protected override void ProtectedInit()
	{
		_keyBindingsLibrarian = new KeyBindingsLibrarian();
		_controlsMessage.SetActive(value: false);
		_controlBindings.SetActive(value: false);
		UpdateButtons();
	}

	protected override void ProtectedDispose()
	{
		_keyBindingsLibrarian = null;
	}

	private void OnUpdatePlayingMusicalInstrument(object[] obj)
	{
		UpdatePlayingMusicalInstrumentEvent updatePlayingMusicalInstrumentEvent = obj[0] as UpdatePlayingMusicalInstrumentEvent;
		switch (updatePlayingMusicalInstrumentEvent.State)
		{
		case MusicalInstrumentScreenState.Playing:
			_leftClickTextMesh.text = "to exit playing.";
			_helpTextMesh.text = "to show controls.";
			_controlsMessage.SetActive(value: true);
			break;
		case MusicalInstrumentScreenState.NotPlaying:
			_leftClickTextMesh.text = "to start playing.";
			_controlsMessage.SetActive(value: false);
			_controlBindings.SetActive(value: false);
			break;
		case MusicalInstrumentScreenState.ShowHelp:
			_helpTextMesh.text = "to hide controls.";
			_controlBindings.SetActive(value: true);
			break;
		case MusicalInstrumentScreenState.HideHelp:
			_helpTextMesh.text = "to show controls.";
			_controlBindings.SetActive(value: false);
			break;
		}
	}

	private void UpdateButtons()
	{
		UpdateButton(InputButtons.UseLeftHand, _leftClickControlTextMesh);
		UpdateButton(InputButtons.InstrumentControls, _helpControlTextMesh);
		UpdateButton(InputButtons.NoteE, _eTextMesh);
		UpdateButton(InputButtons.NoteF, _fTextMesh);
		UpdateButton(InputButtons.NoteG, _gTextMesh);
		UpdateButton(InputButtons.NoteA, _aTextMesh);
		UpdateButton(InputButtons.NoteB, _bTextMesh);
		UpdateButton(InputButtons.NoteC, _cTextMesh);
		UpdateButton(InputButtons.NoteD, _dTextMesh);
		UpdateButton(InputButtons.NoteEOctave, _eOctaveTextMesh);
		UpdateButton(InputButtons.Modifier1, _modifier1TextMesh);
		UpdateButton(InputButtons.Modifier2, _modifier2TextMesh);
	}

	private void UpdateButton(InputButtons button, TextMeshProUGUI textField)
	{
		Dictionary<InputButtons, KeyCode[]> cachedBindings = _keyBindingsLibrarian.CachedBindings;
		cachedBindings.TryGetValue(button, out var value);
		textField.text = ((value[1] != 0) ? value[1].ToString() : value[0].ToString());
	}
}
