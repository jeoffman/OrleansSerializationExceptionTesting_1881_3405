#define CAUSE_THE_TEST_TO_FAIL_WITH_SERIALIZATION_EXCEPTION_SOMETIMES

using System;
using System.Threading.Tasks;
using Contract;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;

namespace GrainsWithStateUsingContracts
{
    [StorageProvider(ProviderName = "StorageProviderName")]
    [ImplicitStreamSubscription("Namespace")]
    public class GrainWithStateUsingContract : Grain<GrainWithStateUsingContractState>, IGrainWithStateUsingContract
    {
        private Logger _logger;

        public override async Task OnActivateAsync()
        {
            _logger = base.GetLogger(nameof(GrainWithStateUsingContract));
            _logger.Info($"OnActivateAsync {this.GetPrimaryKey()}");
            await SubscribeToStream();
        }

        public async Task SubscribeToStream()
        {
            IStreamProvider streamProvider = base.GetStreamProvider("StreamProvider");

            //loop every namespace
            object[] attrs = GetType().GetCustomAttributes(typeof(ImplicitStreamSubscriptionAttribute), true);
            foreach(ImplicitStreamSubscriptionAttribute implictSub in attrs)
            {
                var streamNamespace = implictSub.Namespace;
                var stream = streamProvider.GetStream<MessageContract>(this.GetPrimaryKey(), streamNamespace);
                _logger.Info($"Preparing to subscribe to stream facility={this.GetPrimaryKey()} namespace {streamNamespace}");

                try
                {
                    _logger.Info($"subscribing to StreamProvider->{streamNamespace}");
                    var sub = await stream.SubscribeAsync(
                        (msg, t) =>
                        {
                            _logger.Verbose(0, $"facility={this.GetPrimaryKey()} got an OnNext with msg={msg.GetType().Name} token={t}");
                            return HandleMessage(msg, t);
                        },
                        (e) =>
                        {
                            _logger.Error(0, $"facility={this.GetPrimaryKey()} failed", e);
                            return TaskDone.Done;
                        },
                        () =>
                        {
                            _logger.Warn(0, $"facility={this.GetPrimaryKey()} completed");
                            return TaskDone.Done;
                        }
                    );
                    _logger.Info(0, $"subhandleId {sub.HandleId} on {sub.StreamIdentity} facility={this.GetPrimaryKey()}");
                }
                catch(Exception ex)
                {
                    _logger.Error(0, $"Subscription failure facility={this.GetPrimaryKey()}", ex);
                    throw;
                }
            }
        }

        private async Task HandleMessage(MessageContract msg, StreamSequenceToken t)
        {
            _logger.Info($"facility={this.GetPrimaryKey()} Publishing msg={msg}");

            var orleansStreamProvider = base.GetStreamProvider("StreamProvider");
            var stream = orleansStreamProvider.GetStream<MessageContract>(this.GetPrimaryKey(), "Namespace");
#if CAUSE_THE_TEST_TO_FAIL_WITH_SERIALIZATION_EXCEPTION_SOMETIMES
            //this.State.MessageContract = msg;
#endif
            await WriteStateAsync();
        }

    }

    public class GrainWithStateUsingContractState
    {
#if CAUSE_THE_TEST_TO_FAIL_WITH_SERIALIZATION_EXCEPTION_SOMETIMES
        public MessageContract MessageContract { get; set; }
#endif
    }
}
