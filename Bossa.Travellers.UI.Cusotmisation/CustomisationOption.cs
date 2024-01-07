using System.Collections.Generic;

namespace Bossa.Travellers.UI.Cusotmisation;

public class CustomisationOption : CustomisationSubOption
{
	public Dictionary<string, CustomisationSubOption> subOptions = new Dictionary<string, CustomisationSubOption>();

	public string selectedSubOptionId;
}
