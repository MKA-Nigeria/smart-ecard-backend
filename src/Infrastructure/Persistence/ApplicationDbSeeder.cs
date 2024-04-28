using Application.Common.Persistence;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Authorization;
using Shared.Configurations;

namespace Infrastructure.Persistence.Initialization;
internal class ApplicationDbSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<AppConfiguration> _configRepository;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger, IRepository<AppConfiguration> configRepository)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configRepository = configRepository;
        _seederRunner = seederRunner;
        _logger = logger;
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await SeedConfigurationKeysAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in Roles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogInformation("Seeding {role} Role", roleName);
                role = new ApplicationRole(roleName, $"{roleName} Role");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == Roles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, Permissions.Basic, role);
            }
            else if (roleName == Roles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, Permissions.Admin, role);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyList<Permission> permissions, ApplicationRole role)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == permission.Name))
            {
                _logger.LogInformation("Seeding {role} Permission '{permission}'", role.Name, permission.Name);
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ClaimConstants.Permission,
                    ClaimValue = permission.Name,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com")
            is not ApplicationUser adminUser)
        {
            string adminUserName = $"{"admin"}.{Roles.Admin}".ToLowerInvariant();
            adminUser = new ApplicationUser
            {
                FirstName = "admin",
                LastName = Roles.Admin,
                Email = "admin@gmail.com",
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = "admin@gmail.com"?.ToUpperInvariant(),
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                IsActive = true
            };

            _logger.LogInformation("Seeding Default Admin User");
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, "Password123@");
            await _userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await _userManager.IsInRoleAsync(adminUser, Roles.Admin))
        {
            _logger.LogInformation("Assigning Admin Role to Admin User");
            await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }

    private async Task SeedConfigurationKeysAsync()
    {
        foreach (string key in ConfigurationKeys.DefaultConfigurationKeys)
        {
            if (await _configRepository.FirstOrDefaultAsync(r => r.Key == key)
                is not AppConfiguration config)
            {
                // Create the appConfig
                _logger.LogInformation("Seeding {key} AppConfiguration", key);
                config = new AppConfiguration(key, "change from default");
                await _configRepository.AddAsync(config);
                await _configRepository.SaveChangesAsync();
            }
        }
    }
}