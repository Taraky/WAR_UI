using Travellers.Quests;

namespace Travellers.UI.Events;

public class ConditionalQuestInstructionStateChangeEvent : UIEvent
{
	public readonly QuestInstructionData InstructionData;

	public readonly bool State;

	public ConditionalQuestInstructionStateChangeEvent(QuestInstructionData instructionData, bool state)
	{
		InstructionData = instructionData;
		State = state;
	}
}
