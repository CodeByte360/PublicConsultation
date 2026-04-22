using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using System;

namespace PublicConsultation.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Division> Divisions { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<PoliceStation> PoliceStations { get; set; }
    public DbSet<AiAnalysisResult> AiAnalysisResults { get; set; }
    public DbSet<DraftDocument> DraftDocuments { get; set; }
    public DbSet<Ministry> Ministries { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<Opinion> Opinions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Biometric> Biometrics { get; set; }
    public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
    public DbSet<ChatbotKnowledgeIndex> ChatbotKnowledgeIndex { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
            entity.HasIndex(u => u.NIDNumber).IsUnique();
        });
    }
}
