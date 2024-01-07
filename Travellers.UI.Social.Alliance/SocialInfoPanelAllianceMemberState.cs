using System;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelAllianceMemberState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly AllianceMember _evtAllianceMemberToView;

	private readonly SocialInfoPanelAllianceMember _socialInfoPanelAllianceMember;

	private readonly AllianceMember _yourAllianceMemberInfo;

	public SocialInfoPanelAllianceMemberState(SocialInfoPanelAllianceMember socialInfoPanelAllianceMember, AllianceMember evtAllianceMemberToView, AllianceMember you)
	{
		if (socialInfoPanelAllianceMember == null)
		{
			throw new ArgumentNullException("socialInfoPanelAllianceMember is null in SocialInfoPanelState SocialInfoPanelAllianceMemberState.ctor");
		}
		if (evtAllianceMemberToView == null)
		{
			throw new ArgumentNullException("evtAllianceMemberToView is null in SocialInfoPanelState SocialInfoPanelAllianceMemberState.ctor");
		}
		if (you == null)
		{
			throw new ArgumentNullException("you is null in SocialInfoPanelState SocialInfoPanelAllianceMemberState.ctor");
		}
		_yourAllianceMemberInfo = you;
		_socialInfoPanelAllianceMember = socialInfoPanelAllianceMember;
		_evtAllianceMemberToView = evtAllianceMemberToView;
	}

	public void EnterScreen()
	{
		_socialInfoPanelAllianceMember.SetObjectActive(isActive: true);
		_socialInfoPanelAllianceMember.SetData(_evtAllianceMemberToView, _yourAllianceMemberInfo);
	}

	public void LeaveScreen()
	{
		_socialInfoPanelAllianceMember.SetObjectActive(isActive: false);
	}
}
