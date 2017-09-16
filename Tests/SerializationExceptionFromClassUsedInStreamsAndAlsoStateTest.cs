using Orleans;
using Orleans.Streams;
using System;
using System.Threading.Tasks;
using Contract;
using Xunit;
using Xunit.Abstractions;

namespace OrleansSerializationExceptionTests
{
    [Collection(nameof(RxProofOfConceptTestFixture.RxProofOfConceptTestFixtureFixtureCollection))]
    public class SerializationExceptionFromClassUsedInStreamsAndAlsoStateTest
    {
        private readonly RxProofOfConceptTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public SerializationExceptionFromClassUsedInStreamsAndAlsoStateTest(RxProofOfConceptTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task SerializationExceptionFromClassUsedInStreamsAndAlsoStateTestTest()
        {
            Guid facilityGuid = Guid.NewGuid();

            MessageContract messageReceivedFromStream = null;

            #region Stream setup spy
            IStreamProvider callpointStreamProvider = _fixture.HostedCluster.StreamProviderManager.GetStreamProvider("StreamProvider");
            IAsyncStream<MessageContract> callpointStream = callpointStreamProvider.GetStream<MessageContract>(facilityGuid, "Namespace");
            var subHandle = callpointStream.SubscribeAsync((message, token) =>
            {
                messageReceivedFromStream = message;
                return TaskDone.Done;
            });


            IStreamProvider streamProvider = _fixture.HostedCluster.StreamProviderManager.GetStreamProvider("StreamProviderOther");
            IAsyncStream <SomeOtherMessage> messageStream = streamProvider.GetStream<SomeOtherMessage>(facilityGuid, "NamespaceOther");

            #endregion

            var aceMessage = new SomeOtherMessage
            {
                MyProp = "MyProp"
            };
            await messageStream.OnNextAsync(aceMessage);
            await Task.Delay(TimeSpan.FromMilliseconds(1000));

            Assert.NotNull(messageReceivedFromStream);
            Assert.IsType<MessageContract>(messageReceivedFromStream);
        }
    }
}
