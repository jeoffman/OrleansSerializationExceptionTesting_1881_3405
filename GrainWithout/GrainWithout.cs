using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;
using Contract;

namespace GrainsWithout
{
    [ImplicitStreamSubscription("NamespaceOther")]
    public class GrainWithout : Grain, IGrainWithout
    {
        private Logger _logger;

        public override async Task OnActivateAsync()
        {
            _logger = base.GetLogger(nameof(GrainWithout));
            _logger.Info($"OnActivateAsync {this.GetPrimaryKey()}");
            await SubscribeToStream();
        }

        public async Task SubscribeToStream()
        {
            IStreamProvider streamProvider = base.GetStreamProvider("StreamProviderOther");

            //loop every namespace
            object[] attrs = GetType().GetCustomAttributes(typeof(ImplicitStreamSubscriptionAttribute), true);
            foreach(ImplicitStreamSubscriptionAttribute implictSub in attrs)
            {
                var streamNamespace = implictSub.Namespace;
                var stream = streamProvider.GetStream<SomeOtherMessage>(this.GetPrimaryKey(), streamNamespace);
                _logger.Info($"Preparing to subscribe to stream facility={this.GetPrimaryKey()} namespace {streamNamespace}");

                try
                {
                    _logger.Info($"subscribing to StreamProviderOther->{streamNamespace}");
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

        private async Task HandleMessage(SomeOtherMessage msg, StreamSequenceToken t)
        {
            _logger.Info($"facility={this.GetPrimaryKey()} Publishing msg={msg}");

            var orleansStreamProvider = base.GetStreamProvider("StreamProvider");
            var stream = orleansStreamProvider.GetStream<MessageContract>(this.GetPrimaryKey(), "Namespace");

            MessageContract outboundMessage = new MessageContract
            {
                MyProp = msg.MyProp
            };

            await stream.OnNextAsync(outboundMessage);
        }
    }

    public interface IGrainWithout : IGrainWithGuidKey
    {
    }
}
