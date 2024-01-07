using System.Text;
using Bossa.Travellers.Utils;

namespace Travellers.UI.DebugDisplay;

public class ProductionBuildState : BuildInfoDisplayState
{
	public ProductionBuildState(bool pts, bool debug)
		: base(pts, debug)
	{
	}

	protected override string CreateBuildNumber(VersionInfo versionInfo, string buildString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(buildString);
		stringBuilder.Append("Update ");
		stringBuilder.Append(versionInfo.UpdateNumber);
		if (_pts)
		{
			stringBuilder.Append(".");
			stringBuilder.Append(versionInfo.InternalBuildNumber);
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}
}
