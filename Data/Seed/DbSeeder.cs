using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using OMS.Models.Entities;

namespace OMS.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(OnlineLearningExamSystemContext db)
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Rolename = "admin" },
                new Role { Rolename = "teacher" },
                new Role { Rolename = "student" }
            );
            await db.SaveChangesAsync();
        }

        var adminRoleId = await db.Roles
            .Where(r => r.Rolename == "admin")
            .Select(r => r.Roleid)
            .FirstAsync();

        var adminExists = await db.Users.AnyAsync(u => u.Username == "admin" || u.Email == "admin@gmail.com");
        if (!adminExists)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("admin123"); 

            db.Users.Add(new User
            {
                Username = "admin",
                Passwordhash = hash,
                Fullname = "Administrator",
                Email = "admin@gmail.com",
                Roleid = adminRoleId,
                Isactive = true,
                Avatarurl = null
            });

            await db.SaveChangesAsync();
        }
    }
}