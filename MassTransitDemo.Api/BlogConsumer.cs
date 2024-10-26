using MassTransit;

namespace MassTransitDemo.Api;

public class BlogConsumer : IConsumer<BlogCreated>
{
    private readonly ILogger<BlogConsumer> _logger;
    private readonly DemoDbContext _db;

    public BlogConsumer(ILogger<BlogConsumer> logger, DemoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Consume(ConsumeContext<BlogCreated> context)
    {
        _logger.LogInformation("Blog created: {Text}", context.Message.Id);

        var blog = await _db.Blogs.FindAsync(context.Message.Id);

        blog!.Processed = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

public class BlogEfConsumer //: IConsumer<BlogCreated>
{
    private readonly ILogger<BlogConsumer> _logger;
    private readonly DemoDbContext _db;

    public BlogEfConsumer(ILogger<BlogConsumer> logger, DemoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public Task Consume(ConsumeContext<BlogCreated> context)
    {
        _logger.LogInformation($"{_db.ContextId}");

        return Task.CompletedTask;
    }
}

public class BlogCreated
{
    public Guid Id { get; set; }
}