using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelTextWarningState : ISocialInfoPanelState, ISocialScreenState
{
	private readonly GameObject _allianceLogoBox;

	private readonly SocialInfoPanelAllianceInfo _socialInfoPanelAllianceInfo;

	private readonly SocialInfoPanelAllianceMember _socialInfoPanelAllianceMember;

	private readonly SocialInfoPanelApplicationAndInvitation _socialInfoPanelApplicationAndInvitation;

	private readonly TextStylerTextMeshPro _textWarning;

	public SocialInfoPanelTextWarningState(GameObject allianceLogoBox, TextStylerTextMeshPro textWarning, SocialInfoPanelAllianceInfo socialInfoPanelAllianceInfo, SocialInfoPanelAllianceMember socialInfoPanelAllianceMember, SocialInfoPanelApplicationAndInvitation socialInfoPanelApplicationAndInvitation)
	{
		_allianceLogoBox = allianceLogoBox;
		_textWarning = textWarning;
		_socialInfoPanelAllianceInfo = socialInfoPanelAllianceInfo;
		_socialInfoPanelAllianceMember = socialInfoPanelAllianceMember;
		_socialInfoPanelApplicationAndInvitation = socialInfoPanelApplicationAndInvitation;
	}

	public void EnterScreen()
	{
		_textWarning.SetObjectActive(isActive: false);
		_allianceLogoBox.SetActive(value: true);
		_socialInfoPanelAllianceInfo.SetObjectActive(isActive: false);
		_socialInfoPanelAllianceMember.SetObjectActive(isActive: false);
		_socialInfoPanelApplicationAndInvitation.SetObjectActive(isActive: false);
		_socialInfoPanelApplicationAndInvitation.Setup();
	}

	public void LeaveScreen()
	{
		_allianceLogoBox.SetActive(value: false);
		_textWarning.SetObjectActive(isActive: false);
	}
}
