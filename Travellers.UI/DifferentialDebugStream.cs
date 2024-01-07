using System;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI;

public class DifferentialDebugStream : DebugStream
{
	private double? _previousValue;

	private float _previousTime;

	public DifferentialDebugStream(string name, Color color, float clearTime, Func<float> getTimeFunc)
		: base(name, color, clearTime, getTimeFunc)
	{
	}

	public override void AddPoint(double value)
	{
		if (!base.Enabled)
		{
			return;
		}
		float currentTime = GetCurrentTime();
		try
		{
			if (_previousValue.HasValue)
			{
				double num = value - _previousValue.Value;
				float num2 = currentTime - _previousTime;
				if (num2 <= 0f)
				{
					WALogger.Error<DifferentialDebugStream>("Got bad timeDelta: {0}", new object[1] { num2 });
				}
				else
				{
					double value2 = num / (double)num2;
					base.AddPoint(value2);
				}
			}
		}
		finally
		{
			_previousValue = value;
			_previousTime = currentTime;
		}
	}
}
