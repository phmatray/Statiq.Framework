﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ReplaceDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceDocumentsFixture
        {
            [Test]
            public async Task PipelineReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceCollection serviceCollection = new ServiceCollection()
                    .AddSingleton<ILoggerProvider>(new TestLoggerProvider());
                Engine engine = new Engine(serviceCollection);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d => content.Add(await d.GetStringAsync()))).ForEachDocument();
                engine.Pipelines.Add("Foo", new TestPipeline(new CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new CreateDocuments("E", "F")));
                engine.Pipelines.Add(new TestPipeline(new ReplaceDocuments("Foo"), gatherData).WithDependencies("Foo", "Bar"));

                // When
                await engine.ExecuteAsync(cancellationTokenSource);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task EmptyConstructorWithSpecifiedPipelinesReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                IServiceCollection serviceCollection = new ServiceCollection()
                    .AddSingleton<ILoggerProvider>(new TestLoggerProvider());
                Engine engine = new Engine(serviceCollection);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d => content.Add(await d.GetStringAsync()))).ForEachDocument();
                engine.Pipelines.Add("Foo", new TestPipeline(new CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new CreateDocuments("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new CreateDocuments("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new ReplaceDocuments("Foo", "Baz"), gatherData)
                        .WithDependencies("Foo", "Bar", "Baz"));

                // When
                await engine.ExecuteAsync(cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "G", "H" }, content);
            }

            [Test]
            public async Task SpecifiedPipelineDocumentsAreReturnedInCorrectOrder()
            {
                // Given
                List<string> content = new List<string>();
                IServiceCollection serviceCollection = new ServiceCollection()
                    .AddSingleton<ILoggerProvider>(new TestLoggerProvider());
                Engine engine = new Engine(serviceCollection);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d => content.Add(await d.GetStringAsync()))).ForEachDocument();
                engine.Pipelines.Add("Foo", new TestPipeline(new CreateDocuments("A", "B", "C", "D")));
                engine.Pipelines.Add("Bar", new TestPipeline(new CreateDocuments("E", "F")));
                engine.Pipelines.Add("Baz", new TestPipeline(new CreateDocuments("G", "H")));
                engine.Pipelines.Add(
                    new TestPipeline(new ReplaceDocuments("Baz", "Foo"), gatherData)
                        .WithDependencies("Foo", "Bar", "Baz"));

                // When
                await engine.ExecuteAsync(cancellationTokenSource);

                // Then
                Assert.AreEqual(6, content.Count);
                CollectionAssert.AreEquivalent(new[] { "G", "H", "A", "B", "C", "D" }, content);
            }
        }
    }
}
