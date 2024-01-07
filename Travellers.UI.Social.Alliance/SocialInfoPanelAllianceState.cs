using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelAllianceState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly AllianceBasicInformation _evtAllianceBasicToView;

	private readonly SocialInfoPanelAllianceInfo _socialInfoPanelAllianceInfo;

	public SocialInfoPanelAllianceState(SocialInfoPanelAllianceInfo socialInfoPanelAllianceInfo, AllianceBasicInformation evtAllianceBasicToView)
	{
		_socialInfoPanelAllianceInfo = socialInfoPanelAllianceInfo;
		_evtAllianceBasicToView = evtAllianceBasicToView;
	}

	public void EnterScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: true);
		_socialInfoPanelAllianceInfo.SetForSearchAllianceView(_evtAllianceBasicToView);
	}

	public void LeaveScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: false);
	}
}
