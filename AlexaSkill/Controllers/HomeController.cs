using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AlexaSkill.Models;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using TimeZoneConverter;

namespace AlexaSkill.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private string apiEndpoint = "";
		private string apiAccessToken = "";
		private string apiTimeZoneUrl = "/v2/devices/{deviceId}/settings/System.timeZone";
		private string deviceId = "";
		private string graphAccessToken = "";

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Index()
		{
			ViewData["GetOrPost"] = "GET";
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index([FromBody] AlexaPostRequest alexaPostRequest)
		{
			ViewData["GetOrPost"] = "POST";

			string requestTypeString = "";
			var theGreeting = "";
			var speak = "";

			try
			{
				var jsonContext = JsonDocument.Parse(alexaPostRequest.context.ToString());
				var jsonRequest = JsonDocument.Parse(alexaPostRequest.request.ToString());

				jsonRequest.RootElement.TryGetProperty("type", out var requestType);
				requestTypeString = requestType.GetString();

				jsonContext.RootElement.TryGetProperty("System", out var jsonSystem);

				jsonSystem.TryGetProperty("apiEndpoint", out var jsonApiEndpoint);
				apiEndpoint = jsonApiEndpoint.GetString();
				jsonSystem.TryGetProperty("apiAccessToken", out var jsonApiAccessToken);
				apiAccessToken = jsonApiAccessToken.GetString();

				jsonSystem.TryGetProperty("device", out var jsonDevice);
				jsonDevice.TryGetProperty("deviceId", out var jsonDeviceId);
				deviceId = jsonDeviceId.GetString();

				jsonSystem.TryGetProperty("user", out var jsonUser);
				jsonUser.TryGetProperty("accessToken", out var jsonGraphAccessToken);
				graphAccessToken = jsonGraphAccessToken.GetString();

				switch (requestTypeString)
				{
					case "LaunchRequest":
						{
							var theTimeZone = await GetTimeZone();
							theGreeting = GetGreeting(theTimeZone);
							speak += "Hello World! Joe was here. " + theGreeting;
						}
						break;
					case "IntentRequest":
						{
							jsonRequest.RootElement.TryGetProperty("intent", out var jsonIntent);
							jsonIntent.TryGetProperty("name", out var jsonIntentName);
							var intentName = jsonIntentName.GetString();

							switch (intentName.ToLower())
							{
								case "amazon.helpintent":
									{
										speak += "Please visit MetroShareSolutions.com for help.";
									}
									break;
								case "getteams":
									{
										speak += await GetTeams();
									}
									break;
								case "getmessages":
									{
										speak += await GetMessages();
									}
									break;
								default:
									break;
							}
						}
						break;
					default:
						break;
				}

			}
			catch (Exception ex)
			{
				requestTypeString = ex.Message;
			}

			AlexaPostResponse alexaPostResponse = new AlexaPostResponse();
			alexaPostResponse.version = "1.0";
			//alexaPostResponse.response.directives = new List<AlexaDirective>();

			alexaPostResponse.response = new AlexaPostResponseResponse();
			alexaPostResponse.response.shouldEndSession = false;

			alexaPostResponse.response.outputSpeech = new AlexaOutputSpeech();
			alexaPostResponse.response.outputSpeech.type = "SSML";

			alexaPostResponse.response.outputSpeech.ssml = "<speak>" + speak + "</speak>";

			var jsonResponse = System.Text.Json.JsonSerializer.Serialize(alexaPostResponse);
			ViewData["jsonResponse"] = jsonResponse;

			return View();
		}

		private async Task<string> GetTeams()
		{
			var theReturn = "";

			HttpClient graphClient = new HttpClient();
			graphClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
			graphClient.DefaultRequestHeaders.Authorization = 
				new AuthenticationHeaderValue("Bearer", graphAccessToken);
			
			var theJson = await graphClient.GetAsync("me/joinedTeams");
			var theContent = await theJson.Content.ReadAsStringAsync();

			var theParsed = JsonDocument.Parse(theContent);
			var theValue = theParsed.RootElement.GetProperty("value");
			var enumeratedArray = theValue.EnumerateArray();

			var theList = new List<string>();
			foreach(var item in enumeratedArray)
			{
				theList.Add(item.GetProperty("displayName").GetString());
			}

			theList.Sort();

			for (var i = 0; i < theList.Count; i++)
			{
				theList[i] = "Team Number " + (i + 1) + ", " + theList[i];
			}

			theReturn = string.Join(",", theList);

			return theReturn;
		}

		private async Task<string> GetMessages()
		{
			var theReturn = "";
			var teamId = "6edfeed1-aef5-4042-81a6-f46f0070ea0d";
			var channelId = "19:166b3168043f4fbcb8e1e28ae5961bfd@thread.tacv2";

			HttpClient graphClient = new HttpClient();
			graphClient.BaseAddress = new Uri("https://graph.microsoft.com/beta/");
			graphClient.DefaultRequestHeaders.Authorization = 
				new AuthenticationHeaderValue("Bearer", graphAccessToken);
			
			var theJson = await graphClient.GetAsync("teams/" + teamId + "/channels/" + channelId + "/messages");
			var theContent = await theJson.Content.ReadAsStringAsync();

			var theParsed = JsonDocument.Parse(theContent);
			var theValue = theParsed.RootElement.GetProperty("value");
			var enumeratedArray = theValue.EnumerateArray();

			var theList = new List<string>();
			foreach(var item in enumeratedArray)
			{
				theList.Add(item.GetProperty("displayName").GetString());
			}

			theList.Sort();

			for (var i = 0; i < theList.Count; i++)
			{
				theList[i] = "Team Number " + (i + 1) + ", " + theList[i];
			}

			theReturn = string.Join(",", theList);

			return theReturn;
		}

		private async Task<string> GetTimeZone()
		{
			var theReturn = "";
			var apiTimeZoneUrlDevice = apiTimeZoneUrl.Replace("{deviceId}", deviceId);

			try
			{
				HttpClient alexaClient = new HttpClient();
				alexaClient.BaseAddress = new Uri(apiEndpoint);
				alexaClient.DefaultRequestHeaders.Authorization = 
					new AuthenticationHeaderValue("Bearer", apiAccessToken);
				var theJson = await alexaClient.GetAsync(apiTimeZoneUrlDevice);

				theReturn = await theJson.Content.ReadAsStringAsync();
			}
			catch (Exception ex)
			{
				theReturn = ex.Message;
			}

			return theReturn;
		}

		private string GetGreeting(string theTimeZone)
		{
			var theGreeting = "Hello from Gabe";
			//var stringTimeZoneId = TZConvert.IanaToWindows(theTimeZone.Replace("\"", ""));
			//var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(stringTimeZoneId);
			//var userDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, timeZoneInfo);

			//var hourOfDay = userDateTime.TimeOfDay.Hours;

			//if (hourOfDay >= 4 && hourOfDay <= 11) // 4:00a - 11:59a
			//	theGreeting = "Good Morning!";

			//else if (hourOfDay >= 12 && hourOfDay <= 16) // 12:00p - 4:59p
			//	theGreeting = "Good Afternoon!";

			//else if (hourOfDay >= 17 && hourOfDay <= 23) // 5:00p - 11:59p
			//	theGreeting = "Good Evening.";

			//else // It's the middle of the night
			//	theGreeting = "Hello.";

			return theGreeting;
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
