using System.Text;
using Bossa.Travellers.Utils;

namespace Travellers.UI.DebugDisplay;

public class UnityEditorBuildState : BuildInfoDisplayState
{
	public UnityEditorBuildState(bool pts, bool debug)
		: base(pts, debug)
	{
	}

	protected override string CreateBuildNumber(VersionInfo versionInfo, string buildString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(buildString);
		stringBuilder.AppendLine(BuildNumberUtils.GetGitBranch());
		stringBuilder.Append("Build ");
		stringBuilder.AppendLine(versionInfo.Version);
		return stringBuilder.ToString();
	}
}
