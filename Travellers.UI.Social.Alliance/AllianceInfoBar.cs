using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class AllianceInfoBar : UIScreenComponent
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Sprite _placeholder;

	[SerializeField]
	private UIButtonController _nameButtonController;

	[SerializeField]
	private TextStylerTextMeshPro _ageAndMembers;

	private AllianceBasicInformation _allianceBasicInformation;

	private IAllianceClient _allianceClient;

	private UIColourAndTextReferenceData _uiColourAndTextReferenceData;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient, UIColourAndTextReferenceData colourAndTextReferenceData)
	{
		_uiColourAndTextReferenceData = colourAndTextReferenceData;
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetData(string searchParameter, AllianceBasicInformation allianceBasicInfo)
	{
		_allianceBasicInformation = allianceBasicInfo;
		_icon.sprite = _placeholder;
		if (!string.IsNullOrEmpty(allianceBasicInfo.EmblemWebLink))
		{
			_allianceClient.GetEmblem(allianceBasicInfo).Then(delegate(Sprite sprite)
			{
				if (sprite != null)
				{
					_icon.sprite = sprite;
				}
			});
		}
		Color32 colour = _uiColourAndTextReferenceData.UIColours.GetColour(ColourReference.ColourType.AllianceSearchHighlight);
		string text = allianceBasicInfo.AllianceDisplayName.TryHighlightSelection(searchParameter, colour, returnEmptyIfNotFound: false);
		string text2 = allianceBasicInfo.Description.TryHighlightSelection(searchParameter, colour, returnEmptyIfNotFound: true, 8);
		if (!string.IsNullOrEmpty(text2))
		{
			_nameButtonController.SetText($"{text}\n<i>{text2}</i>");
		}
		else
		{
			_nameButtonController.SetText(text);
		}
		_nameButtonController.SetButtonEvent(WAUIAllianceEvents.AllianceInfoButtonPressed, new SelectAllianceToViewEvent(allianceBasicInfo));
		Epoch epoch = new Epoch(_allianceBasicInformation.FoundingDate);
		_ageAndMembers.SetText($"Founded: {epoch.ToDateTime():dd/MM/yyyy}\nMembers: {_allianceBasicInformation.MemberCount}");
	}
}
