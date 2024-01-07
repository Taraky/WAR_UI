using Tayx.Graphy;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.DebugDisplay;

public class DebugFPSScreen : UIScreen
{
	[SerializeField]
	private GraphyManager _graphyManager;

	[SerializeField]
	private GraphyDebugger _graphyDebugger;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void ProcessFlags(DebugFPSScreenFlags evt)
	{
		SetObjectActive(evt.IsEnabled);
		if (evt.IsEnabled)
		{
			if (evt.ShowAdvanced.HasValue)
			{
				_graphyManager.AdvancedModuleState = ((!evt.ShowAdvanced.Value) ? GraphyManager.ModuleState.OFF : GraphyManager.ModuleState.FULL);
			}
			if (evt.ShowRam.HasValue)
			{
				_graphyManager.RamModuleState = ((!evt.ShowRam.Value) ? GraphyManager.ModuleState.OFF : GraphyManager.ModuleState.FULL);
			}
			if (evt.ShowAudio.HasValue)
			{
				_graphyManager.AdvancedModuleState = ((!evt.ShowAudio.Value) ? GraphyManager.ModuleState.OFF : GraphyManager.ModuleState.FULL);
			}
		}
	}
}
