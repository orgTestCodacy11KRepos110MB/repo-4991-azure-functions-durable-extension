﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Tests
{
    public class DurableTaskListenerTests
    {
        private readonly string functionId = "DurableTaskTriggerFunctionId";
        private readonly FunctionName functionName = new FunctionName("DurableTaskTriggerFunctionName");
        private readonly DurableTaskExtension config;
        private readonly string storageConnectionString;
        private readonly DurableTaskListener listener;

        public DurableTaskListenerTests()
        {
            this.config = GetDurableTaskConfig();
            this.storageConnectionString = TestHelpers.GetStorageConnectionString();
            this.listener = new DurableTaskListener(
                this.config,
                this.functionId,
                this.functionName,
                FunctionType.Activity,
                this.storageConnectionString);
        }

        [Fact]
        [Trait("Category", PlatformSpecificHelpers.TestCategory)]
        public void GetMonitor_ReturnsExpectedValue()
        {
            IScaleMonitor scaleMonitor = this.listener.GetMonitor();

            Assert.Equal(typeof(DurableTaskScaleMonitor), scaleMonitor.GetType());
            Assert.Equal($"{this.functionId}-DurableTaskTrigger-DurableTaskHub".ToLower(), scaleMonitor.Descriptor.Id);

            var scaleMonitor2 = this.listener.GetMonitor();

            Assert.Same(scaleMonitor, scaleMonitor2);
        }

        private static DurableTaskExtension GetDurableTaskConfig()
        {
            var options = new DurableTaskOptions();
            options.HubName = "DurableTaskHub";
            options.WebhookUriProviderOverride = () => new Uri("https://sampleurl.net");
            var wrappedOptions = new OptionsWrapper<DurableTaskOptions>(options);
            var nameResolver = TestHelpers.GetTestNameResolver();
            var storageAccountProvider = new TestStorageAccountProvider();
            var platformInformationService = TestHelpers.GetMockPlatformInformationService();
            var serviceFactory = new AzureStorageDurabilityProviderFactory(
                wrappedOptions,
                storageAccountProvider,
                nameResolver,
                NullLoggerFactory.Instance,
                platformInformationService);
            return new DurableTaskExtension(
                wrappedOptions,
                new LoggerFactory(),
                nameResolver,
                new[] { serviceFactory },
                new TestHostShutdownNotificationService(),
                new DurableHttpMessageHandlerFactory(),
                platformInformationService: platformInformationService);
        }
    }
}
