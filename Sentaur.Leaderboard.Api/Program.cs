using Microsoft.EntityFrameworkCore;
using Sentaur.Leaderboard.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(TimeSpan.FromDays(1));
        });
});

builder.Services.AddDbContextFactory<LeaderboardContext>(
    options =>
        options.UseSqlite());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeaderboardContext>();
    context.Database.Migrate();
    if (!context.ScoreEntries.Any())
    {
        context.ScoreEntries.AddRange(Store.MockScores);
        context.SaveChanges();
    }
}

app.MapGet("/score", (LeaderboardContext context, CancellationToken token) =>
{
    return context.ScoreEntries
        .ToListAsync(token);
})
.WithName("scores")
.WithOpenApi();

app.Run();


