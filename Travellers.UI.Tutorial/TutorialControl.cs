using System;
using TMPro;
using UnityEngine;

namespace Travellers.UI.Tutorial;

public class TutorialControl : MonoBehaviour
{
	[Serializable]
	public class TutorialControlData
	{
		[Serializable]
		public enum ContentType
		{
			LMB,
			RMB,
			ONE_BUTTON,
			TWO_BUTTONS
		}

		public ContentType Type;

		public ContentPosition Anchor = ContentPosition.MIDDLE;

		public string Name = string.Empty;

		public bool Hold;

		public InputButtons[] InputButtons = new InputButtons[0];
	}

	[SerializeField]
	private ContentPosition _controlPosition;

	[SerializeField]
	private GameObject _holdLabel;

	[SerializeField]
	private GameObject _mouseLeft;

	[SerializeField]
	private GameObject _mouseRight;

	[SerializeField]
	private GameObject _firstButton;

	[SerializeField]
	private GameObject _secondButton;

	[SerializeField]
	private TextMeshProUGUI _firstButtonLabel;

	[SerializeField]
	private TextMeshProUGUI _secondButtonLabel;

	[SerializeField]
	private TextMeshProUGUI _actionLabel;

	public ContentPosition ControlPosition => _controlPosition;

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetData(TutorialControlData data, string firstButtonLabel, string secondButtonLabel)
	{
		if (data == null)
		{
			Deactivate();
			return;
		}
		_actionLabel.text = data.Name;
		_holdLabel.SetActive(data.Hold);
		_mouseLeft.SetActive(data.Type == TutorialControlData.ContentType.LMB);
		_mouseRight.SetActive(data.Type == TutorialControlData.ContentType.RMB);
		_firstButton.SetActive(data.Type == TutorialControlData.ContentType.ONE_BUTTON || data.Type == TutorialControlData.ContentType.TWO_BUTTONS);
		_secondButton.SetActive(data.Type == TutorialControlData.ContentType.TWO_BUTTONS);
		_firstButtonLabel.text = firstButtonLabel;
		_secondButtonLabel.text = secondButtonLabel;
		base.gameObject.SetActive(value: true);
	}
}
