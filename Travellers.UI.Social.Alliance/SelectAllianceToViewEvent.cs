using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Alliance;

public class SelectAllianceToViewEvent : UIEvent
{
	public AllianceBasicInformation AllianceBasicToView;

	public SelectAllianceToViewEvent(AllianceBasicInformation allianceBasicToView)
	{
		AllianceBasicToView = allianceBasicToView;
	}
}
