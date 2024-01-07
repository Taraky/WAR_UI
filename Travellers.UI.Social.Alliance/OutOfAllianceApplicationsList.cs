using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class OutOfAllianceApplicationsList : IMembershipListMessageFilter
{
	public void GetMembershipMessages(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetPlayerToAllianceApplications(pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void GetMembershipMessagesWithParams(IAllianceClient allianceClient, string searchTerm, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetPlayerToAllianceApplicationsWithParams(searchTerm, pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void SetDataForMessageChoiceButton(UIToggleGroup buttonToggleGroup, AlliancePlayerInfoButton infoButton, MembershipChangeRequest applicationInfo, IAllianceClient allianceClient, Action<Exception> onFailure)
	{
		allianceClient.GetBasicAllianceInfoForAllianceId(applicationInfo.CharacterGroupTargetId).Then(delegate(AllianceBasicInformation allianceInfo)
		{
			infoButton.SetDataForPersonalApplication(buttonToggleGroup, allianceInfo, applicationInfo);
		}).Catch(onFailure);
	}
}
