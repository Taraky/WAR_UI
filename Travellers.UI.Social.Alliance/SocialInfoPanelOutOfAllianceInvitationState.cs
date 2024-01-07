using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelOutOfAllianceInvitationState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly AllianceBasicInformation _evtAllianceBasicToView;

	private readonly SocialInfoPanelAllianceInfo _socialInfoPanelAllianceInfo;

	private readonly MembershipChangeRequest _allianceApplication;

	public SocialInfoPanelOutOfAllianceInvitationState(SocialInfoPanelAllianceInfo socialInfoPanelAllianceInfo, AllianceBasicInformation evtAllianceBasicToView, MembershipChangeRequest evtAllianceApplication)
	{
		_allianceApplication = evtAllianceApplication;
		_socialInfoPanelAllianceInfo = socialInfoPanelAllianceInfo;
		_evtAllianceBasicToView = evtAllianceBasicToView;
	}

	public void EnterScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: true);
		_socialInfoPanelAllianceInfo.SetForAllianceToPlayerInvitationView(_evtAllianceBasicToView, _allianceApplication);
	}

	public void LeaveScreen()
	{
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: false);
	}
}
