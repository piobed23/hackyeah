using App.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NaprawPusteStringi();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        NaprawPusteStringi();
        return base.SaveChanges();
    }

    private void NaprawPusteStringi()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.Metadata.ClrType == typeof(string)
                    && !prop.Metadata.IsNullable
                    && prop.CurrentValue is null)
                {
                    prop.CurrentValue = "";
                }
            }
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<IdeaCard> IdeaCards => Set<IdeaCard>();
    public DbSet<IdeaStageHistory> IdeaStageHistory => Set<IdeaStageHistory>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Endorsement> Endorsements => Set<Endorsement>();
    public DbSet<OrganizationOffer> OrganizationOffers => Set<OrganizationOffer>();
    public DbSet<ProblemReport> ProblemReports => Set<ProblemReport>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();
    public DbSet<ReviewDecision> ReviewDecisions => Set<ReviewDecision>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<TesterApplication> TesterApplications => Set<TesterApplication>();
    public DbSet<TesterFeedback> TesterFeedbacks => Set<TesterFeedback>();
    public DbSet<ImplementationCard> ImplementationCards => Set<ImplementationCard>();
    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();
    public DbSet<LibraryItemChangeProposal> LibraryItemChangeProposals => Set<LibraryItemChangeProposal>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        b.Entity<Idea>()
            .HasOne(i => i.Autor).WithMany().HasForeignKey(i => i.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<IdeaCard>()
            .HasOne(c => c.Idea).WithOne(i => i.Karta!).HasForeignKey<IdeaCard>(c => c.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<IdeaStageHistory>()
            .HasOne(h => h.Idea).WithMany(i => i.HistoriaEtapow).HasForeignKey(h => h.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<IdeaStageHistory>()
            .HasOne(h => h.Kto).WithMany().HasForeignKey(h => h.KtoId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Call>()
            .HasOne(c => c.Organizator).WithMany().HasForeignKey(c => c.OrganizatorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Application>(e =>
        {
            e.HasOne(a => a.Idea).WithMany(i => i.Wnioski).HasForeignKey(a => a.IdeaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Call).WithMany(c => c.Wnioski).HasForeignKey(a => a.CallId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Autor).WithMany().HasForeignKey(a => a.AutorId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<BudgetItem>()
            .HasOne(x => x.Application).WithMany(a => a.Budzet).HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<ScheduleItem>()
            .HasOne(x => x.Application).WithMany(a => a.Harmonogram).HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Comment>()
            .HasOne(x => x.Idea).WithMany(i => i.Komentarze).HasForeignKey(x => x.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Comment>()
            .HasOne(x => x.Autor).WithMany().HasForeignKey(x => x.AutorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Endorsement>(e =>
        {
            e.HasOne(x => x.Idea).WithMany(i => i.Poparcia).HasForeignKey(x => x.IdeaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Uzytkownik).WithMany().HasForeignKey(x => x.UzytkownikId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.IdeaId, x.UzytkownikId }).IsUnique();
        });

        b.Entity<OrganizationOffer>(e =>
        {
            e.HasOne(x => x.Idea).WithMany(i => i.OfertyOrganizacji).HasForeignKey(x => x.IdeaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.OrganizacjaUser).WithMany().HasForeignKey(x => x.OrganizacjaUserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProblemReport>(e =>
        {
            e.HasOne(x => x.Mieszkaniec).WithMany().HasForeignKey(x => x.MieszkaniecId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PowiazanaIdea).WithMany(i => i.ZgloszeniaProblemow).HasForeignKey(x => x.PowiazanaIdeaId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<AuditLogEntry>()
            .HasOne(x => x.Uzytkownik).WithMany().HasForeignKey(x => x.UzytkownikId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<ReviewDecision>(e =>
        {
            e.HasOne(x => x.IdeaCard).WithMany().HasForeignKey(x => x.IdeaCardId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Application).WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Recenzent).WithMany().HasForeignKey(x => x.RecenzentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PolaczZIdea).WithMany().HasForeignKey(x => x.PolaczZIdeaId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<TestSession>()
            .HasOne(t => t.Idea).WithMany(i => i.SesjeTestowe).HasForeignKey(t => t.IdeaId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TesterApplication>(e =>
        {
            e.HasOne(x => x.TestSession).WithMany(t => t.Zgloszenia).HasForeignKey(x => x.TestSessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Uzytkownik).WithMany().HasForeignKey(x => x.UzytkownikId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<TesterFeedback>()
            .HasOne(x => x.TesterApplication).WithOne(a => a.Opinia!).HasForeignKey<TesterFeedback>(x => x.TesterApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Attachment>()
            .HasIndex(a => new { a.WlascicielTyp, a.WlascicielId });

        b.Entity<Call>().Property(c => c.BudzetMaks).HasConversion<double>();
        b.Entity<Call>().Property(c => c.ProcentWkladuWlasnego).HasConversion<double>();
        b.Entity<BudgetItem>().Property(x => x.KosztJednostkowy).HasConversion<double>();
        b.Entity<BudgetItem>().Property(x => x.KwotaBrutto).HasConversion<double>();
        b.Entity<BudgetItem>().Property(x => x.WkladWlasny).HasConversion<double>();
        b.Entity<Application>().Property(a => a.WnioskowanaKwotaGrantu).HasConversion<double>();
        b.Entity<ScheduleItem>().Property(s => s.KosztDzialania).HasConversion<double>();

        b.Entity<ImplementationCard>(e =>
        {
            e.HasOne(x => x.Idea).WithMany().HasForeignKey(x => x.IdeaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InstytucjaUser).WithMany().HasForeignKey(x => x.InstytucjaUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Budzet).HasConversion<double>();
        });

        b.Entity<LibraryItem>(e =>
        {
            e.HasOne(x => x.ZglaszajacyUser).WithMany().HasForeignKey(x => x.ZglaszajacyUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ZastapionyPrzez).WithMany().HasForeignKey(x => x.ZastapionyPrzezId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.DuplikatOf).WithMany().HasForeignKey(x => x.DuplikatOfId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<LibraryItemChangeProposal>(e =>
        {
            e.HasOne(x => x.LibraryItem).WithMany(l => l.PropozycjeZmian).HasForeignKey(x => x.LibraryItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProponujacyUser).WithMany().HasForeignKey(x => x.ProponujacyUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
