using Travellers.UI.PlayerInventory;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Feedback;

public class FeedbackScanInfosItem : FeedbackItem
{
	private ScannableData _scannableData;

	[SerializeField]
	private FeedbackScanBaseInfo[] _infos;

	public void Setup(ScannableData scannableData)
	{
		_scannableData = scannableData;
		if (_scannableData == null)
		{
			WALogger.Warn<ScanningToolUI>(LogChannel.UI, "Not showing scan feedback popup because scanning data is null, returning.", new object[0]);
			return;
		}
		_scannableData.CheckSchematicOrInventoryItemData();
		bool flag = true;
		for (int i = 0; i < _infos.Length; i++)
		{
			FeedbackScanBaseInfo feedbackScanBaseInfo = _infos[i];
			if (feedbackScanBaseInfo.Setup(_scannableData))
			{
				if (flag)
				{
					flag = false;
					feedbackScanBaseInfo.SetLineVisibility(visible: false);
				}
				else
				{
					feedbackScanBaseInfo.SetLineVisibility(visible: true);
				}
			}
		}
	}
}
