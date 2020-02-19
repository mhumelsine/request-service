using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestService.Sagas
{
    public class RequestStateMap : SagaClassMap<RequestState>
    {
        protected override void Configure(EntityTypeBuilder<RequestState> entity, ModelBuilder model)
        {
            model.HasDefaultSchema("events");

            entity.ToTable("SAGA")
                .HasKey(p => p.CorrelationId);

            entity.Property(p => p.CorrelationId)
                .HasColumnName("UID_SAGA");                

            entity.Property(p => p.CurrentState)
                .HasColumnName("DS_STATE")
                .HasMaxLength(255);

            entity.Property(p => p.ResourceMatchingEventStatus)
                .HasColumnName("AM_RESOURCE_STATUS");
        }
    }
}
