using Eventmi.Core.Models.Event;
using Eventmi.Infrastructure.Data.Contexts;
using Eventmi.Infrastructure.Migrations;
using Eventmi.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace Eventmi.Tests
{
    public class Tests
    {
        private RestClient _client;
        private readonly string _baseUrl = "https://localhost:7236";

        [SetUp]
        public void Setup()
        {
            _client = new RestClient(_baseUrl);
        }

        [Test]
        public async Task GetAllEvents_ReturnsSuccessStatusCode()
        {
            //arrange
            var request = new RestRequest("/Event/All", Method.Get);
            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Add_GetRequest_ReturnsAddView()
        {
            //arrange
            var request = new RestRequest("/Event/Add", Method.Get);
            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Add_PostRequest_AddsNewEventAndRedirects()
        {
            //arrange
            var input = new EventFormModel()
            {
                Name = "test",
                Place = "test place",
                Start = new DateTime(2024, 12, 12, 12, 0, 0),
                End = new DateTime(2024, 12, 12, 16, 0, 0)
            };

            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));

            //act
            var response = await _client.ExecuteAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(CheckEventExists(input.Name), "The event was not added to the database.");
        }
        
        [Test]
        public async Task Details_GetRequest_ShouldReturnDetailedView()
        {
            //arrange
            var eventId = 1;
            var request = new RestRequest($"/Event/Details/{eventId}", Method.Get);
            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Details_GetRequest_ShouldReturnNotFoundIfNoIdIsGiven()
        {
            //arrange
            var request = new RestRequest($"/Event/Details/", Method.Get);
            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Edit_PostRequest_ShouldEditEvent()
        {
            var eventId = 4;
            var dbEvent = GetEventById(eventId);

            var input = new EventFormModel()
            {
                Id = dbEvent.Id,
                End = dbEvent.End,
                Name = $"{dbEvent.Name} UPDATED!!!",
                Start = dbEvent.Start,
                Place = dbEvent.Place
            };

            var request = new RestRequest($"/Event/Edit/{dbEvent.Id}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", input.Id);
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Name", input.Name);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", input.Place);

            //act
            var response = await _client.ExecuteAsync(request);
            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var updatedDbEvent = GetEventById(eventId);
            Assert.That(updatedDbEvent.Name, Is.EqualTo(input.Name));
        }

        [Test]
        public async Task Edit_WithIdMismatch_ShouldReturnNotFound()
        {
            var eventId = 1;
            var dbEvent = GetEventById(eventId);

            var input = new EventFormModel()
            {
                Id = 445,
                End = dbEvent.End,
                Name = $"{dbEvent.Name} UPDATED!!!",
                Start = dbEvent.Start,
                Place = dbEvent.Place
            };

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", input.Id);
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Name", input.Name);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("Place", input.Place);

            var response = await _client.ExecuteAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Edit_PostRequest_ShouldReturnBackTheSameVIewIfModelErrorsArePresent()
        {
            //arrange
            var eventId = 1;
            var dbEvent = GetEventById(eventId);

            var input = new EventFormModel()
            {
                Id = dbEvent.Id,
                End = dbEvent.End,
                Name = dbEvent.Name,
                Start = dbEvent.Start,
                Place = dbEvent.Place
            };

            var request = new RestRequest($"/Event/Edit/{eventId}", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Id", input.Id);
            request.AddParameter("Name", input.Name);
            
            //act
            var response = await _client.ExecuteAsync(request);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task DeleteAction_WithValidId_RedirectsToAllEvents()
        {
            //arrange
            var input = new EventFormModel()
            {
                Name = "Event For Delete",
                Place = "test place",
                Start = new DateTime(2024, 12, 12, 12, 0, 0),
                End = new DateTime(2024, 12, 12, 16, 0, 0)
            };

            var request = new RestRequest("/Event/Add", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            request.AddParameter("Name", input.Name);
            request.AddParameter("Place", input.Place);
            request.AddParameter("Start", input.Start.ToString("MM/dd/yyyy hh:mm tt"));
            request.AddParameter("End", input.End.ToString("MM/dd/yyyy hh:mm tt"));

            await _client.ExecuteAsync(request);

            var eventInDb = GetEventByName(input.Name);
            var eventIdToDelete = eventInDb.Id;

            var deleteRequest = new RestRequest($"/Event/Delete/{eventIdToDelete}", Method.Post);

            //act
            var response = await _client.ExecuteAsync(deleteRequest);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
        }

        [Test]
        public async Task Delete_WithNoId_ShouldReturnNotFound()
        {
            var request = new RestRequest("/Event/Delete/", Method.Post);
            var response = await _client.ExecuteAsync(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        private bool CheckEventExists(string name)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>()
                .UseSqlServer("Server=localhost\\SQLEXPRESS04;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true;").Options;
            using (var context = new EventmiContext(options))
            {
                return context.Events.Any(x => x.Name == name);
            }
        }

        private Event GetEventById(int id)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>()
                .UseSqlServer("Server=localhost\\SQLEXPRESS04;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true;").Options;
            using (var context = new EventmiContext(options))
            {
                return context.Events.FirstOrDefault(x => x.Id == id);
            }
        }

        private Event GetEventByName(string name)
        {
            var options = new DbContextOptionsBuilder<EventmiContext>()
                .UseSqlServer("Server=localhost\\SQLEXPRESS04;Database=Eventmi;Trusted_Connection=True;MultipleActiveResultSets=true;").Options;
            using (var context = new EventmiContext(options))
            {
                return context.Events.FirstOrDefault(x => x.Name == name);
            }
        }
    }
}