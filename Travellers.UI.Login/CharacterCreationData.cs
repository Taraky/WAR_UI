using System;
using System.Collections.Generic;
using Assets.Scripts.Player;

namespace Travellers.UI.Login;

[Serializable]
public class CharacterCreationData
{
	public int Id;

	public string characterUid;

	public string Name;

	public string Server;

	public string serverIdentifier;

	public Dictionary<CharacterSlotType, CharacterCustomisationVisualizer.ItemData> Cosmetics;

	public CharacterUniversalColors UniversalColors;

	public bool isMale;

	public bool seenIntro;

	public bool skippedTutorial;
}
