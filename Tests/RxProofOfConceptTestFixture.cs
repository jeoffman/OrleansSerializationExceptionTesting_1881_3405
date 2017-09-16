using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using OrleansClusterTestHarness;
using System;
using System.Diagnostics;
using Xunit;

namespace OrleansSerializationExceptionTests
{
    public class RxProofOfConceptTestFixture : BaseTestClusterFixture
	{
		protected override TestCluster CreateTestCluster()
		{
			TimeSpan _timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(10);

			var options = new TestClusterOptions(1); //default = 2 nodes in cluster

			options.ClusterConfiguration.AddMemoryStorageProvider("PubSubStore");
		    options.ClusterConfiguration.AddMemoryStorageProvider("StorageProviderName");

            options.ClusterConfiguration.AddSimpleMessageStreamProvider(providerName: "StreamProvider", fireAndForgetDelivery: false);
		    options.ClusterConfiguration.AddSimpleMessageStreamProvider(providerName: "StreamProviderOther", fireAndForgetDelivery: false);

            options.ClusterConfiguration.ApplyToAllNodes(c => c.DefaultTraceLevel = Orleans.Runtime.Severity.Error);
			options.ClusterConfiguration.ApplyToAllNodes(c => c.TraceToConsole = false);
			options.ClusterConfiguration.ApplyToAllNodes(c => c.TraceFileName = string.Empty);
			options.ClusterConfiguration.ApplyToAllNodes(c => c.TraceFilePattern = string.Empty);
			options.ClusterConfiguration.ApplyToAllNodes(c => c.StatisticsWriteLogStatisticsToTable = false);
			options.ClusterConfiguration.Globals.ClientDropTimeout = _timeout;
			options.ClusterConfiguration.UseStartupType<RxProofOfConceptTestStartup>();

			options.ClientConfiguration.AddSimpleMessageStreamProvider("StreamProvider", fireAndForgetDelivery: false);
		    options.ClientConfiguration.AddSimpleMessageStreamProvider("StreamProviderOther", fireAndForgetDelivery: false);
            options.ClientConfiguration.DefaultTraceLevel = Orleans.Runtime.Severity.Error;
			options.ClientConfiguration.TraceToConsole = false;
			options.ClientConfiguration.TraceFileName = string.Empty;
			options.ClientConfiguration.ClientDropTimeout = _timeout;


			return new TestCluster(options);
		}

		[CollectionDefinition(nameof(RxProofOfConceptTestFixtureFixtureCollection))]
		public class RxProofOfConceptTestFixtureFixtureCollection : ICollectionFixture<RxProofOfConceptTestFixture>
		{
			// This class has no code, and is never created. Its purpose is simply
			// to be the place to apply [CollectionDefinition] and all the
			// ICollectionFixture<> interfaces.
		}


		public class RxProofOfConceptTestStartup
		{
			public IServiceProvider ConfigureServices(IServiceCollection services)
			{
                //MOCKS here

                return services.BuildServiceProvider();
			}
		}
	}
}
