using System.Text;
using Bossa.Travellers.Utils;

namespace Travellers.UI.DebugDisplay;

public class DevelopmentBuildState : BuildInfoDisplayState
{
	public DevelopmentBuildState(bool pts, bool debug)
		: base(pts, debug)
	{
	}

	protected override string CreateBuildNumber(VersionInfo versionInfo, string buildString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(buildString);
		stringBuilder.Append("Build ");
		stringBuilder.AppendLine(versionInfo.Version);
		if (_debug)
		{
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}
}
