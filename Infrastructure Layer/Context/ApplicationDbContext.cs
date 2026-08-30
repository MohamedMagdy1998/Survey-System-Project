using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,string>
{
    private readonly IHttpContextAccessor HttpContextAccessor;

    public DbSet<Poll> Polls { get; set; }

    public DbSet<Question> Questions { get; set; }

    public DbSet<Answer> Answers { get; set; }

    public DbSet<Vote> Votes { get; set; }

    public DbSet<VoteAnswer> VoteAnswers { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        HttpContextAccessor = httpContextAccessor;
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var CascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var foreignKey in CascadeFKs)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        var currentUserId = HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.UtcNow;
                entry.Entity.CreatedById = currentUserId ?? string.Empty;

            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = DateTime.UtcNow;
                entry.Entity.UpdatedById = currentUserId ?? string.Empty;
                
            }
        }


        return base.SaveChangesAsync(cancellationToken);
    }



}
