using System;

namespace Travellers.UI.Feedback;

[Serializable]
public struct FeedbackPrefabByType
{
	public FeedbackPrefabType Type;

	public string PrefabName;
}
