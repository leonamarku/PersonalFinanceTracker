using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Core.Entities;
using PersonalFinanceTracker.Core.Enums;

namespace PersonalFinanceTracker.Infrastructure.Data;

/// <summary>EF Core database context — one DbSet per aggregate root.</summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User>        Users        => Set<User>();
    public DbSet<Account>     Accounts     => Set<Account>();
    public DbSet<Category>    Categories   => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget>      Budgets      => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
        });

        // ── Account ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).HasMaxLength(100).IsRequired();
            e.Property(a => a.Balance).HasColumnType("decimal(18,2)");
            e.Property(a => a.Currency).HasMaxLength(3).HasDefaultValue("USD");
            e.HasOne(a => a.User)
             .WithMany(u => u.Accounts)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Category ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Color).HasMaxLength(7).HasDefaultValue("#6366f1");
            e.HasOne(c => c.User)
             .WithMany(u => u.Categories)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Transaction ───────────────────────────────────────────────────────
        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(t => t.Description).HasMaxLength(200).IsRequired();
            e.Property(t => t.Notes).HasMaxLength(500);
            e.HasOne(t => t.Account)
             .WithMany(a => a.Transactions)
             .HasForeignKey(t => t.AccountId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Category)
             .WithMany(c => c.Transactions)
             .HasForeignKey(t => t.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Budget ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.LimitAmount).HasColumnType("decimal(18,2)").IsRequired();
            // Unique: one budget per user/category/month/year
            e.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year }).IsUnique();
            e.HasOne(b => b.User)
             .WithMany(u => u.Budgets)
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Category)
             .WithMany(c => c.Budgets)
             .HasForeignKey(b => b.CategoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed global categories ─────────────────────────────────────────────
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Salary",         Type = TransactionType.Income,  Color = "#22c55e", IsGlobal = true },
            new Category { Id = 2, Name = "Freelance",      Type = TransactionType.Income,  Color = "#16a34a", IsGlobal = true },
            new Category { Id = 3, Name = "Food & Dining",  Type = TransactionType.Expense, Color = "#f97316", IsGlobal = true },
            new Category { Id = 4, Name = "Transport",      Type = TransactionType.Expense, Color = "#3b82f6", IsGlobal = true },
            new Category { Id = 5, Name = "Housing",        Type = TransactionType.Expense, Color = "#8b5cf6", IsGlobal = true },
            new Category { Id = 6, Name = "Entertainment",  Type = TransactionType.Expense, Color = "#ec4899", IsGlobal = true },
            new Category { Id = 7, Name = "Healthcare",     Type = TransactionType.Expense, Color = "#ef4444", IsGlobal = true },
            new Category { Id = 8, Name = "Education",      Type = TransactionType.Expense, Color = "#06b6d4", IsGlobal = true },
            new Category { Id = 9, Name = "Shopping",       Type = TransactionType.Expense, Color = "#f59e0b", IsGlobal = true },
            new Category { Id = 10, Name = "Utilities",     Type = TransactionType.Expense, Color = "#64748b", IsGlobal = true }
        );
    }
}
