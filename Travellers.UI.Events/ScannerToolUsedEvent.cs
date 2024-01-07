using Improbable;
using UnityEngine;

namespace Travellers.UI.Events;

public class ScannerToolUsedEvent : UIEvent
{
	public readonly EntityId ScannedEntityId;

	public readonly GameObject UnderlyingGameObject;

	public ScannerToolUsedEvent(EntityId scannedEntityId, GameObject underlyingGameObject)
	{
		ScannedEntityId = scannedEntityId;
		UnderlyingGameObject = underlyingGameObject;
	}
}
