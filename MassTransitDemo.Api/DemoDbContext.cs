using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MassTransitDemo.Api;


public class DemoDbContext(DbContextOptions<DemoDbContext> options) : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }

    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

public class Blog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime? Processed { get; set; }

    public List<Post> Posts { get; set; } = [];
}

public class Post
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public required Guid BlogId { get; set; }

    public Blog Blog { get; set; } = null!;
}