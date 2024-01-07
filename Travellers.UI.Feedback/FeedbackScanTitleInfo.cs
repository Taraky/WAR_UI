using Bossa.Travellers.Materials;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackScanTitleInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		bool flag = SetupText(data.title);
		if (flag)
		{
			SchematicsRarity rarity = (SchematicsRarity)Mathf.Max((int)data.rarity, 0);
			RarityColourSet rarityColoursForButtonStates = RarityHelper.GetRarityColoursForButtonStates(rarity);
			_label.SetRarityColourSet(rarityColoursForButtonStates);
			if (data.Title == "Unnamed Part" && data.components != null && data.components.Length > 0)
			{
				_label.SetText(data.components[0].componentName);
			}
		}
		return flag;
	}
}
