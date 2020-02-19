using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;

namespace RequestService.Sagas
{
    public class RequestStateDbContext : SagaDbContext
    {
        public RequestStateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new RequestStateMap(); }
        }
    }
}
