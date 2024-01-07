namespace Travellers.UI.Tutorial;

public interface IAddOnlyContainer
{
	void Add(TutorialStep step);

	bool Contains(TutorialStep step);
}
