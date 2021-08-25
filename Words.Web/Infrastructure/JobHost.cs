namespace Words.Web.Infrastructure
{
    using System;
    using System.Web.Hosting;
    using NLog;

    public class JobHost : IRegisteredObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _lock = new();
        private volatile bool _shuttingDown;

        public JobHost()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

        public void DoWork(Action work)
        {
            lock (_lock)
            {
                if (_shuttingDown)
                {
                    return;
                }

                try
                {
                    work();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
    }
}