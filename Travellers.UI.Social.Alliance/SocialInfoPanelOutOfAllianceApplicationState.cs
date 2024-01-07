using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelOutOfAllianceApplicationState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly AllianceBasicInformation _evtAllianceBasicToView;

	private readonly SocialInfoPanelAllianceInfo _socialInfoPanelAllianceInfo;

	private readonly MembershipChangeRequest _allianceApplication;

	public SocialInfoPanelOutOfAllianceApplicationState(SocialInfoPanelAllianceInfo socialInfoPanelAllianceInfo, AllianceBasicInformation evtAllianceBasicToView, MembershipChangeRequest evtAllianceApplication)
	{
		_allianceApplication = evtAllianceApplication;
		_socialInfoPanelAllianceInfo = socialInfoPanelAllianceInfo;
		_evtAllianceBasicToView = evtAllianceBasicToView;
	}

	public void EnterScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: true);
		_socialInfoPanelAllianceInfo.SetForPlayerToAllianceApplicationView(_evtAllianceBasicToView, _allianceApplication);
	}

	public void LeaveScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: false);
	}
}
