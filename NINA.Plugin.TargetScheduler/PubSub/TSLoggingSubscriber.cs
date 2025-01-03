using NINA.Plugin.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler.PubSub {

    public class TSLoggingSubscriber : ISubscriber {

        public Task OnMessageReceived(IMessage message) {
            TSLogger.Info($"Received message:\n{TSMessage.LogReceived(message)}");
            return Task.CompletedTask;
        }
    }
}