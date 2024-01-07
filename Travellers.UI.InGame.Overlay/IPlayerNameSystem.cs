using System.Collections.Generic;
using Travellers.UI.Framework;

namespace Travellers.UI.InGame.Overlay;

[InjectedInterface]
public interface IPlayerNameSystem
{
	PlayerNameInfo YourPlayerNameInfo { get; }

	List<PlayerNameInfo> PlayerInfoList { get; }
}
