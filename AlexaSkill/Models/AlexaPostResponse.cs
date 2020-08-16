using System.Collections.Generic;

namespace AlexaSkill.Models
{
	public class AlexaPostResponse
	{
		public string version { get; set; }
		public AlexaPostResponseResponse response { get; set; }
	}

	public class AlexaPostResponseResponse
	{
		public AlexaOutputSpeech outputSpeech { get; set; }
		public bool shouldEndSession { get; set; }
		public List<AlexaDirective> directives { get; set; }
	}

	public class AlexaOutputSpeech
	{
		public string type { get; set; }
		public string ssml { get; set; }
	}

	public class AlexaDirective
	{
	}
}
