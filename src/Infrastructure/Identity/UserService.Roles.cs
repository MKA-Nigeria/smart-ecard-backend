using Microsoft.EntityFrameworkCore;
using Application.Common.Exceptions;
using Application.Identity.Users;
using Domain.Identity;
using Shared.Authorization;

namespace Infrastructure.Identity;
internal partial class UserService
{
    public async Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var userRoles = new List<UserRoleDto>();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new NotFoundException("User Not Found.");
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
        if (roles is null) throw new NotFoundException("Roles Not Found.");
        foreach (var role in roles)
        {
            userRoles.Add(new UserRoleDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Enabled = await _userManager.IsInRoleAsync(user, role.Name!)
            });
        }

        return userRoles;
    }

    public async Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);

        _ = user ?? throw new NotFoundException("User Not Found.");

        foreach (var userRole in request.UserRoles)
        {
            // Check if Role Exists
            if (await _roleManager.FindByNameAsync(userRole.RoleName!) is not null)
            {
                if (userRole.Enabled)
                {
                    if (!await _userManager.IsInRoleAsync(user, userRole.RoleName!))
                    {
                        await _userManager.AddToRoleAsync(user, userRole.RoleName!);
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, userRole.RoleName!);
                }
            }
        }

        await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id, true));

        return "User Roles Updated Successfully.";
    }
}