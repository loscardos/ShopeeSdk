using Microsoft.EntityFrameworkCore;

namespace ShopeeSdk.Storage.EntityFramework;

public class ShopeeTokenDbContext(DbContextOptions<ShopeeTokenDbContext> options) : DbContext(options)
{
    public DbSet<ShopeeTokenEntity> ShopeeTokens => Set<ShopeeTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShopeeTokenEntity>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => new { token.ShopId, token.MerchantId }).IsUnique();
            entity.Property(token => token.AccessToken).IsRequired();
            entity.Property(token => token.RefreshToken).IsRequired();
        });
    }
}
