using System;
using Bossa.Travellers.BossaNet;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Login;

public class CharacterCreationSelectServer : UIScreenComponent
{
	[SerializeField]
	private Text _serverName;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private Action _action;

	public string ServerName { get; private set; }

	public string ServerIdentifier { get; private set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetData(ServerStatusRecord record, Action action)
	{
		_action = action;
		_canvasGroup.interactable = record.ServerStatus == ServerStatus.up;
		_canvasGroup.alpha = ((record.ServerStatus != 0) ? 0.5f : 1f);
		ServerName = record.DisplayName;
		ServerIdentifier = record.ServerIdentifier;
		_serverName.text = record.GetDisplayText();
	}

	public void Select()
	{
		if (_action != null)
		{
			_action();
		}
	}
}
