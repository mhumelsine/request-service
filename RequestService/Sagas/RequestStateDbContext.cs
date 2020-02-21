using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;

namespace RequestService.Sagas
{

    public class MemoryOptimizedLockStatementProvider : SqlLockStatementProvider
    {
        private const string rowLockStatement = "SELECT * FROM {0}.{1} WHERE UID_SAGA = @p0";
        public MemoryOptimizedLockStatementProvider(string defaultSchema, bool enableSchemaCaching = true) 
            : base(defaultSchema, rowLockStatement, enableSchemaCaching)
        {
        }
    }
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
