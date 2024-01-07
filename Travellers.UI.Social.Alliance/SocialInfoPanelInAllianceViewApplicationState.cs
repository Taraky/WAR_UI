using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelInAllianceViewApplicationState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly MembershipChangeRequest _evtApplicationInfo;

	private readonly SocialInfoPanelApplicationAndInvitation _socialInfoPanelApplicationAndInvitation;

	public SocialInfoPanelInAllianceViewApplicationState(MembershipChangeRequest evtApplicationInfo, SocialInfoPanelApplicationAndInvitation socialInfoPanelApplicationAndInvitation)
	{
		_evtApplicationInfo = evtApplicationInfo;
		_socialInfoPanelApplicationAndInvitation = socialInfoPanelApplicationAndInvitation;
	}

	public void EnterScreen()
	{
		_socialInfoPanelApplicationAndInvitation.SetObjectActive(isActive: true);
		_socialInfoPanelApplicationAndInvitation.SetForIncomingApplication(_evtApplicationInfo);
	}

	public void LeaveScreen()
	{
		_socialInfoPanelApplicationAndInvitation.SetObjectActive(isActive: false);
	}
}
