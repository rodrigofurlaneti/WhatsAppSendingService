using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppSendingService.Domain.Messages;
using WhatsAppSendingService.Domain.Messages.ValueObjects;

namespace WhatsAppSendingService.Infrastructure.Persistence.Configurations;

internal sealed class WhatsAppMessageConfiguration : IEntityTypeConfiguration<WhatsAppMessage>
{
    public void Configure(EntityTypeBuilder<WhatsAppMessage> builder)
    {
        builder.ToTable("WhatsAppMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Recipient)
            .HasConversion(
                vo => vo.Value,
                value => PhoneNumber.Create(value).Value)
            .HasColumnName("Recipient")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasConversion(
                vo => vo.Value,
                value => MessageContent.Create(value).Value)
            .HasColumnName("Body")
            .HasMaxLength(MessageContent.MaxLength)
            .IsRequired();

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.ProviderMessageId).HasMaxLength(128);
        builder.Property(m => m.FailureReason).HasMaxLength(1024);
        builder.Property(m => m.AttemptCount).IsRequired();
        builder.Property(m => m.CreatedOnUtc).IsRequired();
        builder.Property(m => m.SentOnUtc);

        builder.Ignore(m => m.DomainEvents);

        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.CreatedOnUtc);
    }
}
