using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaida.Discord.Gateway
{
    internal class HeartbeatService
    {
        private CancellationTokenSource? _cts;

        public void Start(int intervalMs, Func<Task> sendHeartbeat)
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await sendHeartbeat();
                    await Task.Delay(intervalMs, _cts.Token);
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
        }
    }
}
