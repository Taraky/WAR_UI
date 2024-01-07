using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackScanStatInfo : FeedbackScanBaseInfo
{
	[SerializeField]
	private ItemAttribute[] _statBars;

	public override bool Setup(ScannableData data)
	{
		if (data.OrderedStats != null && data.OrderedStats.Count > 0)
		{
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < _statBars.Length; i++)
			{
				if (i < data.statsBars.Length)
				{
					ScannableData.StatsBar statsBar = data.OrderedStats[i];
					_statBars[i].ShowValues(show: true);
					_statBars[i].SetValue(SchematicData.GetStatTitle(statsBar.statName), statsBar.baseNormalized, statsBar.modifierNormalized, statsBar.maxValue);
				}
				else
				{
					_statBars[i].ShowValues(show: false);
				}
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		return base.gameObject.activeSelf;
	}
}
