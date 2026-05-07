using ChatApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Stream.Schema;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Users> Users =>Set<Users>();
    public DbSet<Conversation>            Conversations            => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<Message>                 Messages                 => Set<Message>();
    public DbSet<MessageReadReceipt>      MessageReadReceipts      => Set<MessageReadReceipt>();
    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


        modelBuilder.Entity<ConversationParticipant>(e =>
        {
            e.HasKey(cp => new { cp.ConversationId, cp.UserId });

            e.HasOne(cp => cp.Conversation)
             .WithMany(c => c.Participants)
             .HasForeignKey(cp => cp.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cp => cp.User)
             .WithMany(u => u.Participants)
             .HasForeignKey(cp => cp.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // LastReadMessage — no cascade to avoid cycles
            e.HasOne(cp => cp.LastReadMessage)
             .WithMany()
             .HasForeignKey(cp => cp.LastReadMessageId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Message ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(e =>
        {
            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict); // keep messages if user deleted

            // Self-referencing reply
            e.HasOne(m => m.ReplyToMessage)
             .WithMany()
             .HasForeignKey(m => m.ReplyToMessageId)
             .OnDelete(DeleteBehavior.SetNull);

            // Performance index for loading conversation messages
            e.HasIndex(m => new { m.ConversationId, m.CreatedAt });
        });

        // ── MessageReadReceipt (composite PK) ───────────────────────────────
        modelBuilder.Entity<MessageReadReceipt>(e =>
        {
            e.HasKey(r => new { r.MessageId, r.UserId });

            e.HasOne(r => r.Message)
             .WithMany(m => m.ReadReceipts)
             .HasForeignKey(r => r.MessageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Soft delete global filter ────────────────────────────────────────
        modelBuilder.Entity<Message>().HasQueryFilter(m => !m.IsDeleted);
    }
}