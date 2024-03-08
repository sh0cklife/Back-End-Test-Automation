using RestSharp.Authenticators;
using RestSharp;
using System.Net;
using Newtonsoft.Json;

namespace RestSharpTestProject
{
    public class GitHubAPITests
    {
        private RestClient client;

        [SetUp]
        public void Setup()
        {
            var options = new RestClientOptions("https://api.github.com")
            {
                //MaxTimeout = 3000,
                Authenticator = new HttpBasicAuthenticator("sh0cklife", "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")
            };

            client = new RestClient(options);
        }

        [Test]
        public void Test_GitHubGetIssuesEndpoint()
        {
            //arrange
            //var client = new RestClient("https://api.github.com");
            var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues", Method.Get);
            request.Timeout = 1000;

            //act
            var response = client.Execute(request);

            //assert
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Test_GitHubGetIssuesEndpoint_MoreValidation()
        {
            //arrange
            //var client = new RestClient("https://api.github.com");
            var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues", Method.Get);
            request.Timeout = 1000;

            //act
            var response = client.Execute(request);
            var issuesObject = JsonConvert.DeserializeObject<List<Issues>>(response.Content);

            //assert
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        //private Issues CreateIssue(string title, string body)
        //{
        //    //arrange
        //    var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues", Method.Post);
        //    request.AddBody(new { body, title });

        //    //act
        //    var response = client.Execute(request);
        //    var issuesObject = JsonConvert.DeserializeObject<Issues>(response.Content);
        //    return issuesObject;
        //}

        //[Test]
        //public void Test_CreateGitHubIssue()
        //{
        //    //Arrange, Act
        //    var issue = CreateIssue("DemoTitle1", "DemoBody1");

        //    //Assert
        //    Assert.AreEqual(issue.title, "DemoTitle1");
        //    Assert.AreEqual(issue.body, "DemoBody1");
        //}

        [Test]
        public void Test_EditGitHubIssue()
        {
            var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues/5262");
            request.AddJsonBody(new
            {
                title = "Changing the name of the issue that I created"
            });

            var response = client.Execute(request, Method.Patch);
            var issue = JsonConvert.DeserializeObject<Issues>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(issue.id, Is.GreaterThan(0));
            Assert.That(response.Content, Is.Not.Empty);
            Assert.That(issue.number, Is.GreaterThan(0));
            Assert.That(issue.title, Is.EqualTo("Changing the name of the issue that I created"));
        }

        [Test]
        public void Test_EditGitHubIssue_AnotherIssue()
        {
            var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues/5264");
            request.AddJsonBody(new
            {
                title = "Changing the name of the issue that I created"
            });

            var response = client.Execute(request, Method.Patch);
            var issue = JsonConvert.DeserializeObject<Issues>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(issue.id, Is.GreaterThan(0));
            Assert.That(response.Content, Is.Not.Empty);
            Assert.That(issue.number, Is.GreaterThan(0));
            Assert.That(issue.title, Is.EqualTo("Changing the name of the issue that I created"));
        }

        [Test]
        public void Test_GetAllIssuesFromARepo()
        {
            var request = new RestRequest("/repos/testnakov/test-nakov-repo/issues");
            var response = client.Execute(request);
            var issues = JsonConvert.DeserializeObject<List<Issues>>(response.Content);

            Assert.That(issues.Count > 1);

            foreach (var issue in issues)
            {
                Assert.That(issue.id, Is.GreaterThan(0));
                Assert.That(issue.number, Is.GreaterThan(0));
                Assert.That(issue.title, Is.Not.Empty);
            }
        }
    }
}