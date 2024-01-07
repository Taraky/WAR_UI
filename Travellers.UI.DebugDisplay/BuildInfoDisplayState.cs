using System.Text;
using Bossa.Travellers.Utils;

namespace Travellers.UI.DebugDisplay;

public abstract class BuildInfoDisplayState
{
	protected const string ReleaseStage = "Build ";

	protected const string ProductionPrefix = "Update ";

	protected readonly bool _pts;

	protected readonly bool _debug;

	protected BuildInfoDisplayState(bool pts, bool debug)
	{
		_pts = pts;
		_debug = debug;
	}

	public string GetBuildNumber()
	{
		VersionInfo versionInfo = BuildNumberUtils.GetVersionInfo();
		StringBuilder stringBuilder = new StringBuilder();
		if (_pts)
		{
			stringBuilder.AppendLine("Public Test Server");
		}
		return CreateBuildNumber(versionInfo, stringBuilder.ToString());
	}

	protected abstract string CreateBuildNumber(VersionInfo versionInfo, string buildString);
}
