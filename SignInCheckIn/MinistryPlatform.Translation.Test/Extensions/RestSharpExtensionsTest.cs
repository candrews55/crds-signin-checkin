﻿using FluentAssertions;
using MinistryPlatform.Translation.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System.Collections.Generic;

namespace MinistryPlatform.Translation.Test.Extensions
{
    public class RestSharpExtensionsTest
    {
        private IRestRequest _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new RestRequest();
        }

        [Test]
        public void TestSetJsonBodySingleItem()
        {
            const string input = "hello world";
            _fixture.SetJsonBody(input);
            _fixture.Parameters.Should()
                .Contain(p => p.Name.Equals("Accept") && p.Value.Equals("application/json"));
            _fixture.Parameters.Should()
                .Contain(
                    p =>
                        p.Type == ParameterType.RequestBody && p.Name.Equals("application/json") &&
                        p.Value.Equals(JsonConvert.SerializeObject(new List<string> {input})));
        }

        [Test]
        public void TestSetJsonBodyCollection()
        {
            var list = new List<string>
            {
                "hello world"
            };
            _fixture.SetJsonBody(list);
            _fixture.Parameters.Should()
                .Contain(p => p.Name.Equals("Accept") && p.Value.Equals("application/json"));
            _fixture.Parameters.Should()
                .Contain(
                    p =>
                        p.Type == ParameterType.RequestBody && p.Name.Equals("application/json") &&
                        p.Value.Equals(JsonConvert.SerializeObject(list)));
        }

    }
}