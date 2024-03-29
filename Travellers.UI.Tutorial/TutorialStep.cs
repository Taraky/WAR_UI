using System;

namespace Travellers.UI.Tutorial;

[Serializable]
public enum TutorialStep
{
	NONE = -1,
	LEARN_SHIPBUILDING,
	FIRST_REVIVAL_CHAMBER,
	GRAPLING_HOOK,
	CLIMBABLE_SURFACE,
	CLIMBING,
	LEAVE_REVIVAL_CHAMBER,
	LOAD_SHIP,
	EDIT_SHIP,
	POST_EDIT_SHIP,
	ENTER_SHIP_BUBBLE,
	EQUIP_SHIP_BUILDER,
	PLACE_SHIP_PART,
	EQUIP_SCANNER,
	FLY_SHIP,
	MOUSE_OVER_HELM,
	MOUSE_OVER_CANNON,
	MOUSE_OVER_REVIVER,
	MOUSE_OVER_GENERATOR,
	MOUSE_OVER_CONTAINER,
	MOUSE_OVER_SHIPYARD,
	MOUSE_OVER_ASSEMBLY_STATION,
	INVENTORY_PICKED_ITEM,
	MOUSE_OVER_SAIL_OPEN,
	MOUSE_OVER_SAIL_CLOSE,
	INVENTORY_ITEM,
	EQUIP_GAUNTLET_REPAIR,
	EQUIP_GAUNTLET_SALVAGE,
	GRAPLING_HOOK_CAN_CLIMB,
	EQUIP_RANGED_WEAPON,
	FIRST_CANNON,
	FIRST_GLIDER,
	FIRST_GLIDER_FLY,
	PICKUP_SHIP_PART,
	INVENTORY_PLACE_ITEM,
	SAVE_SHIP,
	PICKUP_ATLAS_SHARD,
	PICKUP_BOMB,
	PICKUP_ATLAS_LIFTER,
	STAND_UP,
	PICKUP_DEFAULT,
	INTERACT_DEFAULT,
	MOUSE_OVER_SWITCH_ON,
	MOUSE_OVER_SWITCH_OFF,
	MOUSE_OVER_HORN,
	MOUSE_OVER_CORE,
	SCHEMATICS_TAB_ACTIVE,
	ASSEMBLY_STATION_TAB_CLICKED,
	CHARACTER_TAB_ACTIVE,
	KNOWLEDGE_TAB_ACTIVE,
	CREW_TAB_ACTIVE,
	LOGBOOK_TAB_ACTIVE,
	TOTAL,
	START_POPUP_SCANNING,
	START_POPUP_SHIPBUILDING,
	DEATH_SCREEN
}
