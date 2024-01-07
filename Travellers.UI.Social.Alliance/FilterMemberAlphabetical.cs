using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class FilterMemberAlphabetical : AllianceMemberFilterType
{
	public bool Ascending;

	public FilterMemberAlphabetical(bool ascending, bool includeOffline)
	{
		Ascending = ascending;
		IncludeOffline = includeOffline;
	}

	public override void Filter(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembersSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetAllianceMembersAlphabetical(Ascending, IncludeOffline, pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}
}
