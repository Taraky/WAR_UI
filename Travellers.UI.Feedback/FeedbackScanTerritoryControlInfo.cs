using Assets.Visualizers;
using Bossa.Travellers.Visualisers.Islands;
using Improbable;
using Improbable.Collections;
using Improbable.Unity.Core;
using Improbable.Unity.Entity;
using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackScanTerritoryControlInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		Transform transform = LocalPlayer.Instance.ScannerToolVisualizer.ScannerTool.ScannerTool.AttachedTo.Transform;
		TerritoryControlBeaconInteractable component = transform.GetComponent<TerritoryControlBeaconInteractable>();
		if (component != null)
		{
			return SetupTCBText(component);
		}
		IslandVisualiser componentInParent = transform.GetComponentInParent<IslandVisualiser>();
		if (componentInParent != null)
		{
			Option<EntityId> currentTcbEntity = componentInParent.CurrentTcbEntity;
			if (currentTcbEntity.HasValue)
			{
				IEntityObject entityObject = SpatialOS.Universe.Get(currentTcbEntity.Value);
				if (entityObject != null && entityObject.UnderlyingGameObject != null)
				{
					TerritoryControlBeaconInteractable component2 = entityObject.UnderlyingGameObject.GetComponent<TerritoryControlBeaconInteractable>();
					return SetupTCBText(component2);
				}
				return SetupText("TCT Not Found");
			}
		}
		return SetupText(null);
	}

	private bool SetupTCBText(TerritoryControlBeaconInteractable tcb)
	{
		return SetupText(FormatInfo(tcb.AllianceNameModel.Data, tcb.IslandNameModel.Data));
	}

	private static string FormatInfo(Option<string> allianceName, Option<string> name)
	{
		return string.Format("TCT Name: {0}\nTCT Owner: {1}", (!name.HasValue) ? "Unnamed" : name.Value, (!allianceName.HasValue) ? "Unknown" : allianceName.Value);
	}
}
