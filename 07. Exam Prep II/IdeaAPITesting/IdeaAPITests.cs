using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net;
using IdeaAPITesting.Models;


namespace IdeaAPITesting
{
    public class IdeaAPITests
    {
        // Restsharp installation
        private RestClient client;

        // 3 predefiend constants, we need those for many use cases
        private const string BASE_URL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "denisqa@abv.bg";
        private const string PASSWORD = "123456";

        private static string lastIdeaId;

        [OneTimeSetUp]
        public void Setup()
        {
            
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASE_URL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            // this is essential to send requests
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var authClient = new RestClient(BASE_URL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                email, password
            });

            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").ToString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty!");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode}, with data {response.Content}");
            }
        }

        [Test, Order(1)]
        public void CreateNewIdea_WithCorrectData_ShouldSucceed()
        {
            var requestData = new IdeaDTO()
            {
                Title = "Test Title",
                Description = "Test Description"
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
            
        }

        [Test, Order(2)]
        public void GetAllIdeas_ShouldReturnNonEmptyArray()
        {
            //arrange
            var request = new RestRequest("/api/Idea/All");
            
            //act
            var response = client.Execute(request, Method.Get);
            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseDataArray.Length, Is.GreaterThan(0));

            lastIdeaId = responseDataArray[responseDataArray.Length - 1].IdeaId;
        }

        [Test, Order(3)]
        public void EditLastIdea_WithCorrectData_ShouldSucceed()
        {
            var requestData = new IdeaDTO()
            {
                Title = "Edited Test Title",
                Description = "Edited Test Description"
            };

            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", lastIdeaId);
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Put);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Edited successfully"));

        }

        [Test, Order(4)]
        public void DeleteLastIdea_ShouldSucceed()
        {
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastIdeaId);
            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));

        }

        [Test, Order(5)]
        public void CreateNewIdea_WithInvalidData_ShouldFail()
        {
            var requestData = new IdeaDTO()
            {
                Title = "",
                Description = "Test Description"
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditIdea_WithInvalidData_Should_NOT_Succeed()
        {
            var requestData = new IdeaDTO()
            {
                Title = "Edited Test Title",
                Description = "Edited Test Description"
            };

            var request = new RestRequest("/api/Idea/Edit/");
            request.AddQueryParameter("ideaId", "NO SUCH IDEA");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Put);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }

        [Test, Order(7)]
        public void DeleteNonExistingIdea_ShouldFail()
        {
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "NON EXISTING IDEA");
            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }
    }
}