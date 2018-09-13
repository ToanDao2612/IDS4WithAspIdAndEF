using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IDS4WithAspIdAndEF.SampleConsole
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IDS4WithAspIdAndEF.SampleConsole";
            Thread.Sleep(10000);

            MainAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("https://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, "cc.Client", "cc.Client.Secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("Front.API.All");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            Console.WriteLine("\n\n cc.Client");
            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }


            // request token
            tokenClient = new TokenClient(disco.TokenEndpoint, "ro.Client", "ro.Client.Secret");
            tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "Pass123$", "Front.API.All");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            Console.WriteLine("\n\n ro.Client");
            // call api
            client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JArray.Parse(content));
            }
            Console.Read();
        }
    }
}
