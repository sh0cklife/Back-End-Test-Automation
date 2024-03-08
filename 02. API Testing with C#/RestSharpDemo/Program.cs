using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharpDemo;

class Program
{
    static void Main(string[] args)
    {
        var client = new RestClient(new RestClientOptions("https://api.github.com")
        {
            Authenticator = new HttpBasicAuthenticator("sh0cklife", "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")
        });

        var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues", Method.Post);

        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { title = "DemoTitle", body = "DemoBody" });
        
        var response = client.Execute(request);

    }
}

