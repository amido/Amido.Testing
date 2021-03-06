﻿using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Amido.Testing.Http;
using Amido.Testing.SpecFlow.Http;

namespace Amido.Testing.SpecFlow.Tests.Http
{
    [Binding]
    public class SimpleTestRequests
    {
        [Given("I am testing the spec flow extensions")]
        public void NotSetupRequired()
        {
            
        }

        [When("I call google")]
        public void GivenIPerformAGetOnUrl()
        {
            RestClient.RequestUri("http://www.google.com")
                .WithRetries(RetryType.UntilStatusCodeEquals, 200, 3, 1000)
                .WithVerb(HttpMethod.Post)
                .AddAcceptHeader(AcceptHeader.Json)
                .AddAuthorizationHeader("Bearer", "1234")
                .AddContentType(ContentType.Json)
                .AddBody("blah")
                .MakeRequest()
                .StoreResponseOnScenarioContext();

            RestClient.RequestUri("http://www.google.com")
                .WithRetries(RetryType.UntilStatusCodeEquals, 200, 3, 1000)
                .WithVerb(HttpMethod.Post)
                .AddAcceptHeader(AcceptHeader.Json)
                .AddAuthorizationHeader("Bearer", "1234")
                .AddContentType(ContentType.Json)
                .AddBody("blah")
                .MakeRequest()
                .StoreResponseOnScenarioContext();
        }

        [When("I call (.*) at url (.*)")]
        public void GivenIPerformAGetOnUrl(string siteName, string url)
        {
            RestClient.RequestUri(url)
                .WithRetries(RetryType.UntilStatusCodeEquals, 200, 2, 1000)
                .WithVerb(HttpMethod.Get)
                .MakeRequest()
                .StoreResponseOnScenarioContext(siteName);
        }

        [Then("the scenario context responses collection should include (.*) requests")]
        public void VerifyScenarioContextResponses(int numberOfRequestsInCollection)
        {
            var context = ScenarioContextService.GetScenarioResponseDictionary();
            Assert.AreEqual(numberOfRequestsInCollection, context.Count);
        }

        [Then("the last response should be set")]
        public void VerifyLastResponse()
        {
            var context = ScenarioContextService.GetScenarioResponseDictionary();
            Assert.IsNotNull(context.LastResponse);
        }
    }
}
