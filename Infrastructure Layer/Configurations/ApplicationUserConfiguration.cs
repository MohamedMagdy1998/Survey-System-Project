using Domain.Common.Const;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure_Layer.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshTokens");
            rt.WithOwner().HasForeignKey("UserId");

            rt.Property(r => r.ExpiresOn)
                .HasColumnName("Expireson"); 

            rt.Property(r => r.RevokedOn)
                .HasColumnName("RevokedOn");


            //Default Data

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            builder.HasData(new ApplicationUser
            {
                Id = DefaultUsers.AdminId,
                FirstName = "Survey Basket",
                LastName = "Admin",
                UserName = DefaultUsers.AdminEmail,
                NormalizedUserName = DefaultUsers.AdminEmail.ToUpper(),
                Email = DefaultUsers.AdminEmail,
                NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
                SecurityStamp = DefaultUsers.AdminSecurityStamp,
                ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null!, DefaultUsers.AdminPassword)
            });
        });
    }
}
