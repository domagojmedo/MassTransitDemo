using System.Reflection;
using MassTransit;
using MassTransitDemo.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DemoDbContext>(options => options
        .UseNpgsql(builder.Configuration.GetConnectionString(nameof(DemoDbContext)))
        .UseSnakeCaseNamingConvention());

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString(nameof(DemoDbContext));
    });

builder.Services.AddMassTransit(x =>
{
    var entryAssembly = Assembly.GetEntryAssembly();
    x.AddConsumers(entryAssembly);

    x.AddSqlMessageScheduler();

    x.UsingPostgres((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });

    x.AddEntityFrameworkOutbox<DemoDbContext>(o =>
    {
        // configure which database lock provider to use (Postgres, SqlServer, or MySql)
        o.UsePostgres();

        // enable the bus outbox
        o.UseBusOutbox();
    });
});

builder.Services.AddPostgresMigrationHostedService(x => x.CreateDatabase = false);

var app = builder.Build();

app.MapGet("/", () => "OK");

app.MapPost("/create/{count:int?}", async (DemoDbContext db, IPublishEndpoint bus, int? count = 1) =>
{
    var blogs = Enumerable.Range(0, count!.Value)
        .Select(x => new Blog())
        .ToList();

    db.Blogs.AddRange(blogs);

    var blogsCreated = blogs.Select(x => new BlogCreated
    {
        Id = x.Id
    });

    await bus.PublishBatch(blogsCreated);

    var result = await db.SaveChangesAsync();

    return Results.Ok(result);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

app.Run();