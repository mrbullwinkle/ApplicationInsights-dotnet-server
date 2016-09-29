﻿namespace Microsoft.ApplicationInsights.DependencyCollector
{
    using System;
    using Channel;
    using DataContracts;
    using Extensibility;
    using Implementation;

    /// <summary>
    /// Telemetry Initializer that parses http dependencies into well-known types like Azure Storage.
    /// </summary>
    public class HttpDependenciesParsingTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// If telemetry item is http dependency - converts it to the well-known type of the dependency.
        /// </summary>
        /// <param name="telemetry">Telemetry item to convert.</param>
        public void Initialize(ITelemetry telemetry)
        {
            var httpDependency = telemetry as DependencyTelemetry;

            if (httpDependency != null && httpDependency.Type != null && httpDependency.Type.Equals(RemoteDependencyConstants.HTTP, StringComparison.OrdinalIgnoreCase))
            {
                string host = httpDependency.Target;

                if (!string.IsNullOrEmpty(host))
                {
                    if (host.EndsWith("blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
                    {
                        // Blob Service REST API: https://msdn.microsoft.com/en-us/library/azure/dd135733.aspx
                        httpDependency.Type = RemoteDependencyConstants.AzureBlob;

                        string nameWithoutVerb = httpDependency.Name;
                        var verb = GetVerb(httpDependency.Name, out nameWithoutVerb);

                        var isFirstSlash = nameWithoutVerb[0] == '/' ? 1 : 0;
                        var idx = nameWithoutVerb.IndexOf('/', isFirstSlash); // typically first symbol of the path is '/'
                        string container = idx != -1 ? nameWithoutVerb.Substring(isFirstSlash, idx - isFirstSlash) : string.Empty;

                        string account = host.Substring(0, host.IndexOf('.'));

                        httpDependency.Name = verb + account + '/' + container;
                    }

                    ////else if (host.EndsWith("table.core.windows.net", StringComparison.OrdinalIgnoreCase))
                    ////{
                    ////    httpDependency.Type = RemoteDependencyConstants.AzureTable;;
                    ////}
                    ////else if (host.EndsWith("queue.core.windows.net", StringComparison.OrdinalIgnoreCase))
                    ////{
                    ////    httpDependency.Type = RemoteDependencyConstants.AzureQueue;
                    ////}
                }
            }
        }

        private static string GetVerb(string name, out string nameWithoutVerb)
        {
            var result = string.Empty;
            nameWithoutVerb = name;

            var idx = name.IndexOf(' ') + 1;
            if (idx != 0)
            {
                result = name.Substring(0, idx);
                nameWithoutVerb = name.Substring(idx);
            }

            return result;
        }
    }
}