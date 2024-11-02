using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.Persistences;
using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Data.Seed;

public class DataSeeder : IDataSeeder
{
	private readonly UserManager<User> _userManager;
	private readonly RoleManager<Role> _roleManager;

	public DataSeeder(RoleManager<Role> roleManager, UserManager<User> userManager)
	{
		_roleManager = roleManager;
		_userManager = userManager;
	}

	public async Task SeedAllAsync()
	{
		await SeedRoles();
		await SeedUsers();
	}

	private async Task SeedRoles()
	{
		if (await _roleManager.RoleExistsAsync(RoleConsts.RoleName.Admin) == false)
		{
			await _roleManager.CreateAsync(new Role(RoleConsts.RoleName.Admin)
			{
				IsStatic = true
			});
		}

		if (await _roleManager.RoleExistsAsync(RoleConsts.RoleName.User) == false)
		{
			await _roleManager.CreateAsync(new Role(RoleConsts.RoleName.User)
			{
				IsStatic = true,
				IsDefault = true
			});
		}
	}

	private async Task SeedUsers()
	{
		// Seed Admin User
		if (await _userManager.FindByNameAsync(UserConsts.DefaultUsername.Admin) == null)
		{
			var adminUser = new User
			{
				FirstName = "Admin",
				LastName = "Tan",
				UserName = UserConsts.DefaultUsername.Admin,
				Email = "admin@testgk.com",
				SecurityStamp = Guid.NewGuid().ToString()
			};

			var result = await _userManager.CreateAsync(adminUser, "123qwe");

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(adminUser, RoleConsts.RoleName.Admin);
			}

			var normalUser = new User
			{
				FirstName = "User",
				LastName = "Tan",
				UserName = UserConsts.DefaultUsername.User,
				Email = "user@testgk.com",
				SecurityStamp = Guid.NewGuid().ToString()
			};

			result = await _userManager.CreateAsync(normalUser, "123qwe");

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(normalUser, RoleConsts.RoleName.User);
			}
		}
	}
}
