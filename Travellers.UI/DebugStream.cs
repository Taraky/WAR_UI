using System;
using System.Collections.Generic;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI;

public class DebugStream
{
	private int _currentPointNum;

	public bool Enabled { get; set; }

	public string Name { get; private set; }

	public Color Color { get; private set; }

	public List<Vector2> Data { get; set; }

	public float ClearTime { get; set; }

	public Func<float> GetTimeFunc { get; set; }

	public int PointThrottle { get; set; }

	public DebugStream(string name, Color color, float clearTime, Func<float> getTimeFunc)
	{
		Name = name;
		Color = color;
		Data = new List<Vector2>(5);
		ClearTime = clearTime;
		GetTimeFunc = getTimeFunc;
	}

	protected float GetCurrentTime()
	{
		return GetTimeFunc();
	}

	public virtual void AddPoint(double value)
	{
		if (Enabled)
		{
			if (double.IsNaN(value))
			{
				WALogger.Error<DebugStream>("Got NaN value in DebugStream: {0}", new object[1] { Name });
			}
			else if (double.IsPositiveInfinity(value))
			{
				WALogger.Error<DebugStream>("Got Infinity value in DebugStream: {0}", new object[1] { Name });
			}
			else if (double.IsNegativeInfinity(value))
			{
				WALogger.Error<DebugStream>("Got -Infinity value in DebugStream: {0}", new object[1] { Name });
			}
			else if (PointThrottle <= 0 || _currentPointNum++ % PointThrottle == 0)
			{
				Data.Add(new Vector2(GetCurrentTime(), (float)value));
			}
		}
	}
}
