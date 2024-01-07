using System;
using System.Collections.Generic;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.Tutorial;

[Serializable]
[CreateAssetMenu(fileName = "TutorialContent", menuName = "UI/TutorialContent")]
public class TutorialContent : ScriptableObject
{
	[Serializable]
	public class AnchoredPopup
	{
		[SerializeField]
		public TutorialHookType HookType;

		[SerializeField]
		public eAnchorDirection HookDirection;

		[SerializeField]
		public string Message;
	}

	[Serializable]
	public enum Lifespan
	{
		PERMANENT,
		TEMPORARY,
		ONE_SHOT
	}

	public TutorialStep Step;

	public Lifespan Duration;

	public eTutorialState activeState;

	public bool GracePeriodOnly;

	public bool CanShowWhileHavenIsActive;

	public string Title;

	public string Message;

	public List<TutorialControl.TutorialControlData> Controls;

	public List<AnchoredPopup> AnchoredPopups = new List<AnchoredPopup>();

	[HideInInspector]
	public bool HasText;

	[HideInInspector]
	public bool HasPopups;

	[HideInInspector]
	public bool HasControls;

	[HideInInspector]
	public ContentGrouping ContentGrouping;

	[HideInInspector]
	public Dictionary<ContentPosition, TutorialControl.TutorialControlData> ControlPositionLookup;

	public void Initialise()
	{
		CheckIfHasTextOrControls();
		CreateControlLookup();
		ContentGrouping = CheckBranches(HasText);
	}

	private void CheckIfHasTextOrControls()
	{
		HasText = !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Message);
		HasPopups = AnchoredPopups.Count > 0;
		HasControls = Controls != null && Controls.Count > 0;
	}

	private void CreateControlLookup()
	{
		ControlPositionLookup = new Dictionary<ContentPosition, TutorialControl.TutorialControlData>();
		for (int i = 0; i < Enum.GetNames(typeof(ContentPosition)).Length; i++)
		{
			ControlPositionLookup[(ContentPosition)i] = null;
		}
		for (int j = 0; j < Controls.Count; j++)
		{
			ControlPositionLookup[Controls[j].Anchor] = Controls[j];
		}
	}

	private ContentGrouping CheckBranches(bool hasText)
	{
		if (HasLeftControl())
		{
			if (HasCentralControl())
			{
				if (HasRightControl())
				{
					return (!HasText) ? ContentGrouping.AllControls : ContentGrouping.NOTAGROUPING;
				}
				return (!HasText) ? ContentGrouping.CentreAndLeftControlsOnly : ContentGrouping.NOTAGROUPING;
			}
			if (HasRightControl())
			{
				return (!HasText) ? ContentGrouping.LeftAndRightControlsOnly : ContentGrouping.HotbarTextBoth;
			}
			return (!HasText) ? ContentGrouping.LeftControlOnly : ContentGrouping.HotbarTextLeftOnly;
		}
		if (HasRightControl())
		{
			if (HasCentralControl())
			{
				return (!HasText) ? ContentGrouping.CentreAndRightControl : ContentGrouping.NOTAGROUPING;
			}
			return HasText ? ContentGrouping.HotbarTextRightOnly : ContentGrouping.RightControlOnly;
		}
		return (!HasText) ? ContentGrouping.CentreControlOnly : ContentGrouping.NOTAGROUPING;
	}

	private bool HasLeftControl()
	{
		return ControlPositionLookup[ContentPosition.LEFT] != null;
	}

	private bool HasCentralControl()
	{
		return ControlPositionLookup[ContentPosition.MIDDLE] != null;
	}

	private bool HasRightControl()
	{
		return ControlPositionLookup[ContentPosition.RIGHT] != null;
	}
}
