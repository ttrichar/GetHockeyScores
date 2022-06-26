using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Company.Function
{
    public class GetScores
    {
        [FunctionName("GetScores")]
        public virtual async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://statsapi.web.nhl.com/api/v1/");
            
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress + "/teams/21/?expand=team.schedule.previous");
            
            var json = await response.Content.ReadAsStringAsync();

            JObject data = JObject.Parse(json);

            var gameLink = data["teams"][0]["previousGameSchedule"]["dates"][0]["games"][0]["gamePk"].Value<string>();
            
            response = await client.GetAsync(client.BaseAddress + "game/" + gameLink + "/feed/live");

            var gameJson = await response.Content.ReadAsStringAsync();

            JObject gameData = JObject.Parse(gameJson);

            var homeTeam = gameData["gameData"]["teams"]["home"]["abbreviation"].Value<string>();

            var awayTeam = gameData["gameData"]["teams"]["away"]["abbreviation"].Value<string>();

            var lastPlay = gameData["liveData"]["plays"]["allPlays"].Last();

            var timeRemaining = lastPlay["about"]["periodTimeRemaining"].Value<string>();

            var goals = lastPlay["about"]["goals"].Value<JObject>();

            var period = lastPlay["about"]["ordinalNum"].Value<string>();

            var homeScore = goals["home"].Value<string>();

            var awayScore = goals["away"].Value<string>();

            var message = timeRemaining + " " + period + " " + homeTeam + " " + homeScore + " " + awayTeam + " " + awayScore;

            Console.WriteLine(message);

            // var location = data["teams"][0]["locationName"].Value<string>();

            // Console.WriteLine(data["teams"][0]["locationName"].Value<string>());

            byte[] ba = Encoding.Default.GetBytes(message);

            var hexString = BitConverter.ToString(ba);

            hexString = hexString.Replace("-", "");

            var myclient = new RestClient("https://rockblock.rock7.com/rockblock/MT?username=" + Environment.GetEnvironmentVariable("User") + "&imei=300434065066450&password=" + Environment.GetEnvironmentVariable("Password") +"&data=" + hexString);

            var request = new RestRequest(resource: (string)null, Method.Post);

            request.AddHeader("Accept", "text/plain");

            // RestResponse iridiumResponse = myclient.Execute(request);
        }
    }
}
