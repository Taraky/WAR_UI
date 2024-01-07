using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public abstract class AllianceMemberFilterType
{
	public bool IncludeOffline;

	public void SetOffline(bool includeOffline)
	{
		IncludeOffline = includeOffline;
	}

	public abstract void Filter(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembersSlice> onSuccess, Action<Exception> onFailure);
}
