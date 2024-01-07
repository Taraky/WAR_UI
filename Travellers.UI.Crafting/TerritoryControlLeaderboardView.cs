using System.Collections.Generic;
using Bossa.Travellers.Territorycontrol;
using UnityEngine;

namespace Travellers.UI.Crafting;

public class TerritoryControlLeaderboardView : MonoBehaviour
{
	[SerializeField]
	private RectTransform _leaderboardEntriesParent;

	[SerializeField]
	private LeaderboardEntryView _leaderboardEntryPrefab;

	[SerializeField]
	private GameObject _errorParent;

	[SerializeField]
	private GameObject _leaderboardParent;

	private List<LeaderboardEntryView> _pooledLeaderboardEntryViews = new List<LeaderboardEntryView>();

	private List<LeaderboardEntryView> _allocatedLeaderboardEntryViews = new List<LeaderboardEntryView>();

	public void SetLeaderboard(LeaderboardResponse data)
	{
		_leaderboardParent.SetActive(value: true);
		_errorParent.SetActive(value: false);
		Clear();
		int rank = 0;
		foreach (LeaderboardEntry entry in data.entries)
		{
			LeaderboardEntryView leaderboardEntryView = GetLeaderboardEntryView();
			leaderboardEntryView.Set(rank, entry);
			leaderboardEntryView.transform.SetSiblingIndex(rank++);
		}
	}

	public void SetError()
	{
		_leaderboardParent.SetActive(value: false);
		_errorParent.SetActive(value: true);
	}

	private void Clear()
	{
		foreach (LeaderboardEntryView allocatedLeaderboardEntryView in _allocatedLeaderboardEntryViews)
		{
			allocatedLeaderboardEntryView.gameObject.SetActive(value: false);
		}
		_pooledLeaderboardEntryViews.AddRange(_allocatedLeaderboardEntryViews);
		_allocatedLeaderboardEntryViews.Clear();
	}

	private LeaderboardEntryView GetLeaderboardEntryView()
	{
		LeaderboardEntryView leaderboardEntryView;
		if (_pooledLeaderboardEntryViews.Count > 0)
		{
			int index = _pooledLeaderboardEntryViews.Count - 1;
			leaderboardEntryView = _pooledLeaderboardEntryViews[index];
			_pooledLeaderboardEntryViews.RemoveAt(index);
		}
		else
		{
			leaderboardEntryView = Object.Instantiate(_leaderboardEntryPrefab, _leaderboardEntriesParent);
		}
		leaderboardEntryView.gameObject.SetActive(value: true);
		_allocatedLeaderboardEntryViews.Add(leaderboardEntryView);
		return leaderboardEntryView;
	}
}
