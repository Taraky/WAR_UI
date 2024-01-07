using Bossa.Travellers.Errors;
using Bossa.Travellers.Game;
using Improbable.Collections;
using WAUtilities.Logging;

namespace Travellers.UI.InfoPopups;

public class ModalPopupTemplates
{
	public static Bossa.Travellers.Game.ModalPopupMessage GetPopupMessageFromError(ModalErrorPopupMessage error)
	{
		string arg = string.Empty;
		string header = string.Empty;
		int buttonsBitmask = 0;
		switch (error.errorCode)
		{
		case Bossa.Travellers.Errors.ErrorCode.FailedParsingLoginMetadata:
		case Bossa.Travellers.Errors.ErrorCode.ParsedLoginMetadataIsNotDefined:
			header = "Login error!";
			arg = "An error occurred while processing your login request.";
			buttonsBitmask = 14;
			break;
		case Bossa.Travellers.Errors.ErrorCode.RejectedConnectionServerInMaintenance:
			header = "Server in maintenance!";
			arg = "Unfortunately the server is currently under maintenance. Please try again later.";
			buttonsBitmask = 6;
			break;
		case Bossa.Travellers.Errors.ErrorCode.CharacterProfileIsUnavailable:
			header = "Login error!";
			arg = "Unfortunately there was an error loading your information from our server. Please try again or contact customer support if the issue persists.";
			buttonsBitmask = 14;
			break;
		case Bossa.Travellers.Errors.ErrorCode.AccountBlacklistedWithReason:
			header = "Account banned!";
			arg = $"Your account has been banned for the following reason: {error.message}. Please contact customer support about any inquiries regarding this issue.";
			buttonsBitmask = 6;
			break;
		case Bossa.Travellers.Errors.ErrorCode.AccountBlacklistedWithoutReason:
			header = "Account banned!";
			arg = "Your account has been banned. Please contact customer support about any inquiries regarding this issue.";
			buttonsBitmask = 6;
			break;
		case Bossa.Travellers.Errors.ErrorCode.AnotherPlayerLoggedInSameAccount:
			header = "Duplicated login detected!";
			arg = "Another user logged in to Worlds Adrift with this account. If you believe your account has been hacked, please contact customer support.";
			buttonsBitmask = 6;
			break;
		case Bossa.Travellers.Errors.ErrorCode.CharacterAlreadyDisconnected:
			WALogger.Error<ModalPopupTemplates>("Trying to open a popup with the error CharacterAlreadyDisconnected, but this error should never reach this code. Please verify how this happened in GSIM.");
			break;
		case Bossa.Travellers.Errors.ErrorCode.LoginRequestTimedOut:
			header = "Login error!";
			arg = "Your request to log in took too long to be processed. Please try again later or contact customer support if the issue persists.";
			buttonsBitmask = 14;
			break;
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateWithBookkeepingApp:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateWithAncientRespawnerAppForClosestRespawner:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateWithAncientRespawnerAppForRandomRespawner:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateWithAncientRespawnerAppForBestHavenIsland:
			header = "Login error!";
			arg = "An internal error occurred with your login request. Please contact customer support with your character name and this error code.";
			buttonsBitmask = 6;
			break;
		case Bossa.Travellers.Errors.ErrorCode.FailedToSetShipStatusOnlineDuringLoginMessageTimedOut:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateReviverGetVisualGlobalPosition:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateShipGetLatestControlPoint:
		case Bossa.Travellers.Errors.ErrorCode.ShipNotVisualAfterMaximumAttempts:
		case Bossa.Travellers.Errors.ErrorCode.ReviverNotVisualAfterMaximumAttempts:
			header = "Login error!";
			arg = "An error occurred while trying to log in your ship. You can contact customer support with your character name and this error code, or in case this error persists, you can abandon your ship and spawn at a random Ancient Respawner.";
			buttonsBitmask = 30;
			break;
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateGetVisualGlobalPositionButParentTypeInvalid:
		case Bossa.Travellers.Errors.ErrorCode.FailedToCommunicateGetLatestControlPointButParentTypeInvalid:
		case Bossa.Travellers.Errors.ErrorCode.GotVisualGlobalPositionButParentTypeInvalid:
		case Bossa.Travellers.Errors.ErrorCode.GotLatestControlPointButParentTypeInvalid:
			header = "Login error!";
			arg = "An internal error occurred while trying to log you in. You can contact customer support with your character name and this error code, or in case this error persists, you can abandon your ship and spawn at a random Ancient Respawner.";
			buttonsBitmask = 30;
			break;
		case Bossa.Travellers.Errors.ErrorCode.InternalExceptionCaughtDuringLoginFlow:
			header = "Login error!";
			arg = "A critical internal error occurred with your login request. Please contact customer support with your character name and this error code.";
			buttonsBitmask = 6;
			break;
		}
		return new Bossa.Travellers.Game.ModalPopupMessage($"{arg} Error: [{(int)error.errorCode}]", header, GetPopupButtonsFromBitmask(buttonsBitmask));
	}

	private static List<ModalPopupButton> GetPopupButtonsFromBitmask(int buttonsBitmask)
	{
		List<ModalPopupButton> list = new List<ModalPopupButton>();
		if (ContainsButton(buttonsBitmask, ModalPopupButtonAction.Retry))
		{
			list.Add(new ModalPopupButton("RETRY", ModalPopupButtonAction.Retry));
		}
		if (ContainsButton(buttonsBitmask, ModalPopupButtonAction.CleanData))
		{
			list.Add(new ModalPopupButton("ABANDON SHIP AND RETRY", ModalPopupButtonAction.CleanData));
		}
		if (ContainsButton(buttonsBitmask, ModalPopupButtonAction.BackToMenu))
		{
			list.Add(new ModalPopupButton("BACK TO MENU", ModalPopupButtonAction.BackToMenu));
		}
		if (ContainsButton(buttonsBitmask, ModalPopupButtonAction.Quit))
		{
			list.Add(new ModalPopupButton("QUIT", ModalPopupButtonAction.Quit));
		}
		return list;
	}

	private static bool ContainsButton(int buttonsBitmask, ModalPopupButtonAction action)
	{
		return ((uint)buttonsBitmask & (uint)action) == (uint)action;
	}
}
