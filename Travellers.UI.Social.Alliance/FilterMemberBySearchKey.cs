using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class FilterMemberBySearchKey : AllianceMemberFilterType
{
	public string SearchTerm;

	public FilterMemberBySearchKey(string searchTerm, bool includeOffline)
	{
		SearchTerm = searchTerm;
		IncludeOffline = includeOffline;
	}

	public override void Filter(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembersSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetAllianceMembersStringMatch(SearchTerm, IncludeOffline, pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}
}
