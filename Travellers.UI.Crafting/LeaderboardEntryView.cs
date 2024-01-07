using Bossa.Travellers.Territorycontrol;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Crafting;

public class LeaderboardEntryView : MonoBehaviour
{
	[SerializeField]
	private Text _rankText;

	[SerializeField]
	private Text _allianceNameText;

	[SerializeField]
	private Text _numOwnedIslands;

	public void Set(int rank, LeaderboardEntry leaderboardEntry)
	{
		_rankText.text = $"#{rank + 1:N0}";
		_allianceNameText.text = leaderboardEntry.allianceName;
		_numOwnedIslands.text = leaderboardEntry.numOwnedIslands.ToString();
	}
}
