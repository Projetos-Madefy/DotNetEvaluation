using DotNetEvaluation.Domain.Constants;
using DotNetEvaluation.Domain.Entities;
using DotNetEvaluation.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetEvaluation.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator =
            new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.AddRange(
                new TodoList
                {
                    Title = "Todo List",
                    Items =
                    {
                        new TodoItem { Title = "Make a todo list 📃" },
                        new TodoItem { Title = "Check off the first item ✅" },
                        new TodoItem { Title = "Realise you've already done two things on the list! 🤯" },
                        new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                    }
                },
                new TodoList
                {
                    Title = "Home 🏠",
                    Items =
                    {
                        new TodoItem { Title = "Buy groceries" },
                        new TodoItem { Title = "Clean kitchen", Done = true },
                        new TodoItem { Title = "Fix the leaking faucet" },
                        new TodoItem { Title = "Organize wardrobe" },
                        new TodoItem { Title = "Water plants", Done = true },
                        new TodoItem { Title = "Walk the dog" },
                    }
                },
                new TodoList
                {
                    Title = "Work 💼",
                    Items =
                    {
                        new TodoItem { Title = "Answer support tickets" },
                        new TodoItem { Title = "Write API docs", Done = true },
                        new TodoItem { Title = "Refactor authentication module" },
                        new TodoItem { Title = "Prepare sprint demo" },
                        new TodoItem { Title = "Review PR #1421" },
                        new TodoItem { Title = "Fix flaky unit tests", Done = true },
                        new TodoItem { Title = "Plan next sprint backlog" },
                        new TodoItem { Title = "Create health checks for services" },
                    }
                },
                new TodoList
                {
                    Title = "Study 📚",
                    Items =
                    {
                        new TodoItem { Title = "Finish Angular 19 signals module" },
                        new TodoItem { Title = "Practice RxJS marble tests" },
                        new TodoItem { Title = "Read about Clean Architecture" },
                        new TodoItem { Title = "Solve 5 LeetCode problems", Done = true },
                        new TodoItem { Title = "Watch EF Core performance talk" },
                        new TodoItem { Title = "Try out Tailwind plugins" },
                        new TodoItem { Title = "Create POC with .NET 9 AOT" },
                        new TodoItem { Title = "Review PostgreSQL indexing strategies" },
                        new TodoItem { Title = "Build small gRPC sample" },
                        new TodoItem { Title = "Document personal notes" },
                    }
                }
            );

            var bigList = new TodoList { Title = "Big List (for pagination) 🔢" };
            for (int i = 1; i <= 35; i++)
            {
                bigList.Items.Add(new TodoItem { Title = $"Task #{i:00}" });
            }

            _context.TodoLists.Add(bigList);

            await _context.SaveChangesAsync();
        }
    }
}
