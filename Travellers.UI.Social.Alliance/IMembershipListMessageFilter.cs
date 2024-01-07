using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public interface IMembershipListMessageFilter
{
	void GetMembershipMessages(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure);

	void GetMembershipMessagesWithParams(IAllianceClient allianceClient, string searchTerm, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure);

	void SetDataForMessageChoiceButton(UIToggleGroup buttonToggleGroup, AlliancePlayerInfoButton infoButton, MembershipChangeRequest applicationInfo, IAllianceClient allianceClient, Action<Exception> onFailure);
}
