using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.API.Models;

namespace POS.API.Data;

public class AppDbContext : IdentityDbContext<UserRecord>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<BranchSettings> BranchSettings => Set<BranchSettings>();
    public DbSet<RefreshTokenEntry> RefreshTokens => Set<RefreshTokenEntry>();
    public DbSet<PasswordResetEntry> PasswordResets => Set<PasswordResetEntry>();
    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionLine> PrescriptionLines => Set<PrescriptionLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(64);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<RefreshTokenEntry>(entity =>
        {
            entity.HasKey(x => x.Token);
            entity.Property(x => x.Token).IsRequired().HasMaxLength(128);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<PasswordResetEntry>(entity =>
        {
            entity.HasKey(x => x.Token);
            entity.Property(x => x.Token).IsRequired().HasMaxLength(128);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
        });

        modelBuilder.Entity<Drug>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Form).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Category).IsRequired().HasMaxLength(100);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(x => x.SellingPrice).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => new { x.BranchId, x.IsActive });
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptNo).IsRequired().HasMaxLength(50);
            entity.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(x => x.CashierId).IsRequired();
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(20);
            entity.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Discount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Tax).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Total).HasColumnType("decimal(18,2)");
            entity.HasMany(x => x.Items).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.BranchId, x.CreatedAt });
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DrugName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RxNumber).IsRequired().HasMaxLength(50);
            entity.Property(x => x.PatientName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.DoctorName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(20);
            entity.HasMany(x => x.Lines).WithOne(x => x.Prescription).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.BranchId, x.Status });
        });

        modelBuilder.Entity<PrescriptionLine>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DrugName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Phone).IsRequired().HasMaxLength(20);
            entity.Property(x => x.Gender).IsRequired().HasMaxLength(10);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.Allergies)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");
            entity.HasIndex(x => new { x.BranchId, x.LastName });
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ContactPerson).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Phone).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.HasMany(x => x.Orders).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.BranchId, x.IsActive });
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PoNumber).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(20);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.Total).HasColumnType("decimal(18,2)");
            entity.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DrugName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<BranchSettings>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BranchId).IsRequired().HasMaxLength(64);
            entity.Property(x => x.PharmacyName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.BranchName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.VatRate).HasColumnType("decimal(5,2)");
            entity.HasIndex(x => x.BranchId).IsUnique();
        });
    }
}
