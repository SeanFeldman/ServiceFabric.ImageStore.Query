using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessOne
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessOne : StatelessService
    {
        public StatelessOne(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var fabricClient = new FabricClient())
            {
                var manifest = await fabricClient.ClusterManager.GetClusterManifestAsync();
                var xdoc = XDocument.Parse(manifest);
                var ns = xdoc.Root.GetDefaultNamespace();
                var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                xmlNamespaceManager.AddNamespace("x", ns.NamespaceName);
                var element = xdoc.XPathSelectElement(@"//x:Parameter[@Name='ImageStoreConnectionString']", xmlNamespaceManager);
                var value = element.Attribute("Value")?.Value;
                ServiceEventSource.Current.ServiceMessage(Context, $"connection string: {value}");
            }

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
