using MassTransit;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RequestService
{
    public class Service : IHostedService
    {
        private readonly IBusControl busControl;

        public Service(IBusControl busControl)
        {
            this.busControl = busControl;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //start the bus
            await busControl.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //stop the bus
            await busControl.StopAsync(cancellationToken);
        }
    }
}
