using TMPro;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Crew;

public struct ScreenFields
{
	public UIButtonController BeaconButton;

	public Image BeaconProgress;

	public TextMeshProUGUI BeaconTimer;

	public GameObject HasCrewLeftPane;

	public GameObject HasCrewRightPane;

	public GameObject InvitePanel;

	public GameObject BeaconPanel;

	public GameObject NoCrewObject;

	public ScreenFields(UIButtonController beaconButton, Image beaconProgress, TextMeshProUGUI beaconTimer, GameObject hasCrewLeftPane, GameObject hasCrewRightPane, GameObject invitePanel, GameObject beaconPanel, GameObject noCrewObject)
	{
		BeaconButton = beaconButton;
		BeaconProgress = beaconProgress;
		BeaconTimer = beaconTimer;
		HasCrewLeftPane = hasCrewLeftPane;
		HasCrewRightPane = hasCrewRightPane;
		InvitePanel = invitePanel;
		BeaconPanel = beaconPanel;
		NoCrewObject = noCrewObject;
	}
}
