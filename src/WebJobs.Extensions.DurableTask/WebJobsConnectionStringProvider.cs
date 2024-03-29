﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask
{
    /// <summary>
    /// Connection string provider which resolves connection strings from the WebJobs context.
    /// </summary>
    [Obsolete("Please use WebJobsConnectionInfoProvider instead.")]
    public class WebJobsConnectionStringProvider : IConnectionStringResolver
    {
#if !FUNCTIONS_V1
        private readonly IConfiguration hostConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebJobsConnectionStringProvider"/> class.
        /// </summary>
        /// <param name="hostConfiguration">A <see cref="IConfiguration"/> object provided by the WebJobs host.</param>
        public WebJobsConnectionStringProvider(IConfiguration hostConfiguration)
        {
            this.hostConfiguration = hostConfiguration ?? throw new ArgumentNullException(nameof(hostConfiguration));
        }

        /// <inheritdoc />
        public string Resolve(string connectionStringName)
        {
            return this.hostConfiguration.GetWebJobsConnectionString(connectionStringName);
        }
#else
        /// <inheritdoc />
        public string Resolve(string connectionStringName)
        {
            return Microsoft.Azure.WebJobs.Host.AmbientConnectionStringProvider.Instance.GetConnectionString(connectionStringName);
        }
#endif
    }
}