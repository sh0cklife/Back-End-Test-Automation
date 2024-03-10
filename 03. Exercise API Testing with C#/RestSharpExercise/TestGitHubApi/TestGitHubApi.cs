using RestSharpServices;
using System.Net;
using System.Reflection.Emit;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using NUnit.Framework.Internal;
using RestSharpServices.Models;
using System;
using System.Net.WebSockets;

namespace TestGitHubApi
{
    public class TestGitHubApi
    {
        private GitHubApiClient client;
        private static string repo;
        private static int lastCreatedIssueNumber;
        private static int lastCreatedCommentId;

        [SetUp]
        public void Setup()
        {
            client = new GitHubApiClient("https://api.github.com/repos/testnakov/",
                "sh0cklife",
                "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");
                repo = "test-nakov-repo";
        }


        [Test, Order (1)]
        public void Test_GetAllIssuesFromARepo()
        {
            var issues = client.GetAllIssues(repo);

            Assert.That(issues, Has.Count.GreaterThan(1), "There should be more than one issue");

            foreach (var issue in issues)
            {
                Assert.That(issue.Id, Is.GreaterThan(0), "Issue ID should be greater than 0");
                Assert.That(issue.Number, Is.GreaterThan(0), "Issue Number should be greater than 0");
                Assert.That(issue.Title, Is.Not.Empty, "Issue Title should not be empty");
            }
        }

        [Test, Order (2)]
        public void Test_GetIssueByValidNumber()
        {
            //arrange
            var issueNumber = 1;
            //act
            var issue = client.GetIssueByNumber(repo, issueNumber);
            //Assert
            Assert.That(issue, Is.Not.Null, "The response should contain issue data.");
            Assert.That(issue.Id, Is.GreaterThan(0), "Issue ID should be greater than 0.");
            Assert.That(issue.Title, Is.Not.Empty, "Issue Title should not be empty.");
            Assert.That(issue.Number, Is.EqualTo(issueNumber), "Issue number should match the requested number.");

        }
        
        [Test, Order (3)]
        public void Test_GetAllLabelsForIssue()
        {
            //arrange
            var issueNumber = 6;

            //act
            var labels = client.GetAllLabelsForIssue(repo, issueNumber);

            //assert
            Assert.That(labels.Count, Is.GreaterThan(0));

            foreach (var label in labels)
            {
                Assert.That(label.Id, Is.GreaterThan(0), "Label ID should be greater than 0.");
                Assert.That(label.Name, Is.Not.Empty, "Label ID should be greater than 0.");

                //Printing the body of each label
                Console.WriteLine("Label: " + label.Id + " - Name: " + label.Name);
            }

        }

        [Test, Order (4)]
        public void Test_GetAllCommentsForIssue()
        {
            //arrange
            var issueNumber = 6;

            //act
            var comments = client.GetAllCommentsForIssue(repo, issueNumber);

            //assert
            Assert.That(comments.Count, Is.GreaterThan(0));

            foreach (var comment in comments)
            {
                Assert.That(comment.Id, Is.GreaterThan(0), "Comment ID should be greater than 0.");
                Assert.That(comment.Body, Is.Not.Empty, "Comment Body is not empty.");

                //Printing the body of each label
                Console.WriteLine("Comment: " + comment.Id + " - Body: " + comment.Body);
            }
        }

        [Test, Order(5)]
        public void Test_CreateGitHubIssue()
        {
            //Arrange
            string title = "New issue Title";
            string body = "Comment in the body";

            //Act
            var issue = client.CreateIssue(repo, title, body);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(issue.Id, Is.GreaterThan(0));
                Assert.That(issue.Number, Is.GreaterThan(0));
                Assert.That(issue.Title, Is.Not.Empty);
                Assert.That(issue.Title, Is.EqualTo(title));
                
            });

            Console.WriteLine(issue.Number);
            lastCreatedIssueNumber = issue.Number;
        }

        [Test, Order (6)]
        public void Test_CreateCommentOnGitHubIssue()
        {
            //Arrange
            int issueNumber = lastCreatedIssueNumber;
            string body = "Hello my dudes, how's it going?";

            //Act
            var comment = client.CreateCommentOnGitHubIssue(repo, issueNumber, body);

            //Assert
            Assert.That(comment.Body, Is.EqualTo(body));

            Console.WriteLine(comment.Id);
            lastCreatedCommentId = comment.Id;
        }

        [Test, Order (7)]
        public void Test_GetCommentById()
        {
            //Arrange
            int commentId = lastCreatedCommentId;
            
            //Act
            var comment = client.GetCommentById(repo, commentId);

            //Assert
            Assert.That(comment.Body, Is.Not.Empty);
            Assert.That(comment.Body, Is.Not.Null);
            Assert.That(comment.Id, Is.EqualTo(commentId));
            Assert.That(comment.Body, Is.EqualTo("Hello my dudes, how's it going?"));
        }


        [Test, Order (8)]
        public void Test_EditCommentOnGitHubIssue()
        {
            //Arrange
            int commentId = lastCreatedCommentId;
            string newBody = "EDITED comment my dudes.";
            //Act
            var editedComment = client.EditCommentOnGitHubIssue(repo, commentId, newBody);
            //Assert
            Assert.That(editedComment, Is.Not.Null);
            Assert.That(editedComment.Id, Is.EqualTo(commentId));
            Assert.That(editedComment.Body, Is.EqualTo(newBody));
        }

        [Test, Order (9)]
        public void Test_DeleteCommentOnGitHubIssue()
        {
            int commentId = lastCreatedCommentId;

            var result = client.DeleteCommentOnGitHubIssue(repo, commentId);

            Assert.That(result, Is.True);
        }


    }
}

