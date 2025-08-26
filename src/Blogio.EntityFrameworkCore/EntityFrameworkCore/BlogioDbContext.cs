using Blogio;
using Blogio.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Blogio.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class BlogioDbContext :
    AbpDbContext<BlogioDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<BlogPostTag> BlogPostTags { get; set; }
    public DbSet<BlogPostLike> BlogPostLikes { get; set; } 
    public DbSet<BlogPostDraft> BlogPostDrafts { get; set; }
    public DbSet<BlogPostVersion> BlogPostVersions { get; set; }
    public DbSet<BlogPostDraftTag> BlogPostDraftTags { get; set; }     
    public DbSet<BlogPostVersionTag> BlogPostVersionTags { get; set; }


    public BlogioDbContext(DbContextOptions<BlogioDbContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        builder.Entity<BlogPost>(b =>
        {
            b.ToTable("BlogPosts");
            b.ConfigureByConvention();
            b.Property(p => p.Title).HasMaxLength(256).IsRequired();
        });

        //BlogPostTag composite key
        builder.Entity<BlogPostTag>()
            .HasKey(x => new { x.BlogPostId, x.TagId });

        // BlogPost -> Comments (1-n)
        builder.Entity<Comment>(b =>
        {
            b.ToTable("Comments");
            b.ConfigureByConvention();
            b.HasOne(c => c.BlogPost)
             .WithMany(p => p.Comments)
             .HasForeignKey(c => c.BlogPostId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BlogPost <-> Tag (n-n)
        builder.Entity<BlogPostTag>(b =>
        {
            b.ToTable("BlogPostTags");
            b.ConfigureByConvention();
            b.HasKey(x => new { x.BlogPostId, x.TagId });
            b.HasOne(pt => pt.BlogPost).WithMany(p => p.BlogPostTags).HasForeignKey(pt => pt.BlogPostId);
            b.HasOne(pt => pt.Tag).WithMany(t => t.BlogPostTags).HasForeignKey(pt => pt.TagId);
        });


        // OnModelCreating:
        builder.Entity<BlogPostLike>(b =>
        {
            b.ToTable("BlogPostLikes");
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.BlogPostId, x.UserId }).IsUnique();
            b.Property(x => x.BlogPostId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
        });


        // DRAFT
        builder.Entity<BlogPostDraft>(b =>
        {
            b.ToTable("BlogPostDrafts");
            b.ConfigureByConvention();
            b.HasOne(d => d.BlogPost)
             .WithMany(p => p.Drafts)
             .HasForeignKey(d => d.BlogPostId)
             .OnDelete(DeleteBehavior.Cascade);
            // Tag ilişkileri...
        });

        // DRAFT <-> TAG (N-N)
        builder.Entity<BlogPostDraftTag>(b =>
        {
            b.ToTable("BlogPostDraftTags");
            b.ConfigureByConvention();

            b.HasKey(x => new { x.BlogPostDraftId, x.TagId });

            b.HasOne(x => x.Draft)
             .WithMany(d => d.Tags)                
             .HasForeignKey(x => x.BlogPostDraftId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Tag)
             .WithMany(t => t.DraftTags)           
             .HasForeignKey(x => x.TagId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // VERSION (geçmiş)
        builder.Entity<BlogPostVersion>(b =>
        {
            b.ToTable("BlogPostVersions");
            b.ConfigureByConvention();
            b.HasOne(v => v.BlogPost)
             .WithMany(p => p.Versions)
             .HasForeignKey(v => v.BlogPostId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(v => new { v.BlogPostId, v.Version }).IsUnique(); 
        });

        // VERSION <-> TAG (N-N)
        builder.Entity<BlogPostVersionTag>(b =>
        {
            b.ToTable("BlogPostVersionTags");
            b.ConfigureByConvention();

            b.HasKey(x => new { x.BlogPostVersionId, x.TagId });

            b.HasOne(x => x.Version)
             .WithMany(v => v.BlogPostVersionTags)                
             .HasForeignKey(x => x.BlogPostVersionId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Tag)
             .WithMany(t => t.VersionTags)         
             .HasForeignKey(x => x.TagId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag
        builder.Entity<Tag>(b =>
        {
            b.ToTable("Tags");
            b.ConfigureByConvention();
            b.Property(t => t.Name).HasMaxLength(128).IsRequired();
            b.HasIndex(t => t.Name).IsUnique();
        });
    }
}

