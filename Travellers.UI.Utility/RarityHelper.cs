using System.Collections.Generic;
using Bossa.Travellers.Materials;

namespace Travellers.UI.Utility;

public static class RarityHelper
{
	private static readonly Dictionary<SchematicsRarity, RarityColourSet> _rarityButtonColourStateLookup = new Dictionary<SchematicsRarity, RarityColourSet>
	{
		{
			SchematicsRarity.Tier1,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.CommonRarityDefault,
				SelectedColourType = ColourReference.ColourType.CommonRaritySelected
			}
		},
		{
			SchematicsRarity.Tier2,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.UncommonRarityDefault,
				SelectedColourType = ColourReference.ColourType.UncommonRaritySelected
			}
		},
		{
			SchematicsRarity.Tier3,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RareRarityDefault,
				SelectedColourType = ColourReference.ColourType.RareRaritySelected
			}
		},
		{
			SchematicsRarity.Tier4,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.ExoticRarityDefault,
				SelectedColourType = ColourReference.ColourType.ExoticRaritySelected
			}
		},
		{
			SchematicsRarity.Tier5,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.PristineRarityDefault,
				SelectedColourType = ColourReference.ColourType.PristineRaritySelected
			}
		},
		{
			SchematicsRarity.Tier6,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.LegendaryRarityDefault,
				SelectedColourType = ColourReference.ColourType.LegendaryRaritySelected
			}
		}
	};

	private static readonly Dictionary<SchematicsRarity, RarityColourSet> _inventoryItemRarityColourSets = new Dictionary<SchematicsRarity, RarityColourSet>
	{
		{
			SchematicsRarity.Tier1,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameCommon,
				SelectedColourType = ColourReference.ColourType.RarityFrameCommonOver
			}
		},
		{
			SchematicsRarity.Tier2,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameUncommon,
				SelectedColourType = ColourReference.ColourType.RarityFrameUncommonOver
			}
		},
		{
			SchematicsRarity.Tier3,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameRare,
				SelectedColourType = ColourReference.ColourType.RarityFrameRareOver
			}
		},
		{
			SchematicsRarity.Tier4,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameExotic,
				SelectedColourType = ColourReference.ColourType.RarityFrameExoticOver
			}
		},
		{
			SchematicsRarity.Tier5,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameMythic,
				SelectedColourType = ColourReference.ColourType.RarityFrameMythicOver
			}
		},
		{
			SchematicsRarity.Tier6,
			new RarityColourSet
			{
				DefaultColourType = ColourReference.ColourType.RarityFrameLegendary,
				SelectedColourType = ColourReference.ColourType.RarityFrameLegendaryOver
			}
		}
	};

	public static RarityColourSet GetRarityColoursForButtonStates(SchematicsRarity rarity)
	{
		return _rarityButtonColourStateLookup[rarity];
	}

	public static RarityColourSet GetColourSetForItems(SchematicsRarity rarity)
	{
		return _inventoryItemRarityColourSets[rarity];
	}

	public static ColourReference.ColourType GetMainRarityColour(SchematicsRarity rarity)
	{
		return _rarityButtonColourStateLookup[rarity].DefaultColourType;
	}

	public static string FormatRarity(SchematicsRarity rarity)
	{
		return rarity switch
		{
			SchematicsRarity.Tier1 => "Common", 
			SchematicsRarity.Tier2 => "Uncommon", 
			SchematicsRarity.Tier3 => "Rare", 
			SchematicsRarity.Tier4 => "Exotic", 
			SchematicsRarity.Tier5 => "Pristine", 
			SchematicsRarity.Tier6 => "Legendary", 
			_ => rarity.ToString(), 
		};
	}
}
