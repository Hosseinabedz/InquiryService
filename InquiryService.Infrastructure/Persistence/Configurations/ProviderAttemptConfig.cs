using InquiryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InquiryService.Infrastructure.Persistence.Configurations
{
    public class ProviderAttemptConfig : IEntityTypeConfiguration<ProviderAttempt>
    {
        public void Configure(EntityTypeBuilder<ProviderAttempt> builder)
        {
            builder.ToTable("ProviderAttempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.InquiryId)
                .IsRequired();

            builder.Property(x => x.ProviderName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.StartedAt)
                .IsRequired();

            builder.Property(x => x.CompletedAt);
        }
    }
}
