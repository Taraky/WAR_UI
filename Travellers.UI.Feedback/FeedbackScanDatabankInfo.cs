using System.Collections.Generic;
using Bossa.Travellers.Visualisers.Islands;
using Bossa.Travellers.Visualisers.Profile;
using Bossa.Travellers.World;
using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackScanDatabankInfo : FeedbackScanTextInfo
{
	private HashSet<long> _duplicateChecker = new HashSet<long>();

	public override bool Setup(ScannableData data)
	{
		if (!LocalPlayer.Exists)
		{
			return SetupText(null);
		}
		AttachCoord attachedTo = LocalPlayer.Instance.ScannerToolVisualizer.ScannerTool.ScannerTool.AttachedTo;
		Transform transform = attachedTo.Transform;
		if (transform == null)
		{
			return SetupText(null);
		}
		int scannedDatabanks = 0;
		int totalDatabanks = 0;
		float? closestUnscannedDistance = null;
		PlayerPropertiesVisualiser playerProperties = LocalPlayer.Instance.playerProperties;
		IslandVisualiser componentInParent = transform.GetComponentInParent<IslandVisualiser>();
		if (componentInParent == null)
		{
			return SetupText(null);
		}
		GetDatabanksByIslandVisualiser(playerProperties, componentInParent, attachedTo.Position, out scannedDatabanks, out totalDatabanks, out closestUnscannedDistance);
		if (totalDatabanks > 0)
		{
			if (closestUnscannedDistance.HasValue)
			{
				return SetupText($"Databanks scanned: {scannedDatabanks}/{totalDatabanks}\nClosest Databank: {closestUnscannedDistance.Value:N0}m");
			}
			return SetupText($"Databanks scanned: {scannedDatabanks}/{totalDatabanks}");
		}
		return SetupText(null);
	}

	private bool CheckAlreadyScannedDatabank(PlayerPropertiesVisualiser playerProperties, long entityId)
	{
		return playerProperties.AlreadyScannedDatabanks.Contains($"ScannableRuin{entityId}");
	}

	private void GetDatabanksByIslandVisualiser(PlayerPropertiesVisualiser playerProperties, IslandVisualiser islandVisualiser, Vector3 attachPoint, out int scannedDatabanks, out int totalDatabanks, out float? closestUnscannedDistance)
	{
		totalDatabanks = 0;
		scannedDatabanks = 0;
		closestUnscannedDistance = null;
		if (!(islandVisualiser != null))
		{
			return;
		}
		List<IslandDatabank> islandDatabanks = islandVisualiser.GetIslandDatabanks();
		_duplicateChecker.Clear();
		for (int i = 0; i < islandDatabanks.Count; i++)
		{
			IslandDatabank islandDatabank = islandDatabanks[i];
			if (!_duplicateChecker.Contains(islandDatabank.databankId.Id))
			{
				_duplicateChecker.Add(islandDatabank.databankId.Id);
				totalDatabanks++;
				if (CheckAlreadyScannedDatabank(playerProperties, islandDatabank.databankId.Id))
				{
					scannedDatabanks++;
				}
				else
				{
					UpdateClosestDatabankDistance(attachPoint, islandVisualiser.transform.position - islandDatabank.relativePosition.ToUnityVector3(), ref closestUnscannedDistance);
				}
			}
		}
	}

	private static void UpdateClosestDatabankDistance(Vector3 attachPoint, Vector3 databankPos, ref float? closestUnscannedDistance)
	{
		float num = Vector3.Distance(databankPos, attachPoint);
		closestUnscannedDistance = ((!closestUnscannedDistance.HasValue) ? num : Mathf.Min(num, closestUnscannedDistance.Value));
	}
}
