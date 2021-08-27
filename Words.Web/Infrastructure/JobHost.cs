namespace Words.Web.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using NLog;

    public class JobHost : IRegisteredObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public JobHost()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            cancellationTokenSource.Cancel();
            HostingEnvironment.UnregisterObject(this);
        }

        public async Task DoWork(Func<CancellationToken, Task> work)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await work.Invoke(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Demystify());
            }
        }
    }
}