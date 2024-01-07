using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI;

public class DebugGraphScreen : UIScreen
{
	[SerializeField]
	private WMG_Axis_Graph _graph;

	[SerializeField]
	private Button _closeButton;

	private GameObject _currentTarget;

	private DebugStream[] _streams;

	private WMG_Series[] _series;

	public GameObject CurrentTarget
	{
		get
		{
			return _currentTarget;
		}
		set
		{
			if (!(_currentTarget != value))
			{
				return;
			}
			if (_streams != null)
			{
				for (int i = 0; i < _streams.Length; i++)
				{
					_streams[i].Enabled = false;
				}
			}
			_graph.deleteAllSeries();
			_currentTarget = value;
			if (_currentTarget == null)
			{
				return;
			}
			IDebugStreamProvider[] array = _currentTarget.GetComponentsInChildren<IDebugStreamProvider>();
			if (array.Length == 0)
			{
				array = _currentTarget.GetComponentsInParent<IDebugStreamProvider>();
			}
			_streams = array.SelectMany(delegate(IDebugStreamProvider provider)
			{
				if (provider.DebugStreams == null)
				{
					WALogger.Error<DebugGraphScreen>("Null DebugStream[] returned by {0}", new object[1] { provider.GetType() });
					return Enumerable.Empty<DebugStream>();
				}
				return provider.DebugStreams;
			}).ToArray();
			_series = new WMG_Series[_streams.Length];
			for (int j = 0; j < _streams.Length; j++)
			{
				DebugStream debugStream = _streams[j];
				debugStream.Enabled = true;
				_series[j] = _graph.addSeries();
				_series[j].seriesName = debugStream.Name;
				_series[j].lineColor = debugStream.Color;
				_series[j].hidePoints = true;
			}
		}
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
		_closeButton.onClick.AddListener(OnCloseButtonClicked);
		if (_streams != null)
		{
			DebugStream[] streams = _streams;
			foreach (DebugStream debugStream in streams)
			{
				debugStream.Enabled = true;
			}
			for (int j = 0; j < _series.Length; j++)
			{
				_series[j].pointValues.Clear();
				_streams[j].Data.Clear();
			}
		}
		StartCoroutine(CR_UpdateGraph());
	}

	private void OnCloseButtonClicked()
	{
		UIWindowController.PopState<DebugGraphState>();
	}

	protected override void Deactivate()
	{
		if (_streams != null)
		{
			DebugStream[] streams = _streams;
			foreach (DebugStream debugStream in streams)
			{
				debugStream.Enabled = false;
			}
		}
	}

	[UsedImplicitly]
	private IEnumerator CR_UpdateGraph()
	{
		while (base.enabled)
		{
			if (_streams != null)
			{
				for (int i = 0; i < _series.Length; i++)
				{
					WMG_Series wMG_Series = _series[i];
					List<Vector2> data = _streams[i].Data;
					if (wMG_Series.pointValues.Count > 0)
					{
						float num = _streams[i].GetTimeFunc();
						float clearTime = num - _streams[i].ClearTime;
						wMG_Series.pointValues.RemoveAll((Vector2 vector2) => vector2.x < clearTime);
					}
					wMG_Series.pointValues.AddRange(data);
					data.Clear();
				}
			}
			yield return null;
		}
	}
}
