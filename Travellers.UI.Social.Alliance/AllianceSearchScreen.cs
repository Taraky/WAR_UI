using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AllianceSearchScreen : UIScreenComponent
{
	[SerializeField]
	private TMP_InputField _searchAllianceInputField;

	[SerializeField]
	private UIButtonController _searchAllianceButton;

	[SerializeField]
	private AllianceList _allianceList;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnAllianceDataChanged);
	}

	private void OnAllianceDataChanged(object[] obj)
	{
		CheckAllianceList();
	}

	protected override void ProtectedInit()
	{
		_searchAllianceButton.SetButtonEvent(OnSearchButtonPressed);
	}

	protected override void ProtectedDispose()
	{
	}

	public void CheckAllianceList()
	{
		_allianceList.RefreshResultsPage(0);
	}

	private void OnSearchButtonPressed()
	{
		_allianceList.SetSearchParameter(_searchAllianceInputField.text);
	}
}
