// JekirdekCase.Data.SeedData.cs
using JekirdekCase.Models;
using Microsoft.AspNetCore.Identity; // PasswordHasher için
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace JekirdekCase.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                if (!context.Customers.Any())
                {
                    context.Customers.AddRange(
                        new Customer
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@example.com",
                            Region = "North America",
                            RegistrationDate = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc)
                        },
                        new Customer
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            Email = "jane.smith@example.com",
                            Region = "Europe",
                            RegistrationDate = new DateTime(2023, 5, 10, 0, 0, 0, DateTimeKind.Utc)
                        },
                        new Customer
                        {
                            FirstName = "Carlos",
                            LastName = "Gomez",
                            Email = "carlos.gomez@example.com",
                            Region = "South America",
                            RegistrationDate = new DateTime(2023, 7, 22, 0, 0, 0, DateTimeKind.Utc)
                        }
                    );
                    try
                    {
                        context.SaveChanges();
                        Console.WriteLine(">>> Mock customers seeded successfully.");
                    }
                    catch (Exception ex_customer)
                    {
                        Console.WriteLine($"!!! Error seeding mock customers: {ex_customer.Message}");
                    }
                }
                else
                {
                    Console.WriteLine(">>> Customers table already has data. Skipping mock customer seeding.");
                }
                if (!context.Users.Any(u => u.Username == "admin"))
                {
                    var passwordHasher = new PasswordHasher<User>(); // User modelimiz için bir hasher oluştur
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = passwordHasher.HashPassword(null, "JekirdekCase123!"),
                        Role = "Admin", // Standart bir rol ismi kullanalım
                        Email = "admin@example.com",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.Users.Add(adminUser);
                    try
                    {
                        context.SaveChanges();
                        Console.WriteLine(">>> Admin user seeded successfully.");
                    }
                    catch (Exception ex_admin)
                    {
                        Console.WriteLine($"!!! Error seeding admin user: {ex_admin.Message}");
                    }
                }
                else
                {
                    Console.WriteLine(">>> Users table already has an admin user or data. Skipping admin user seeding.");
                }
            }
        }
    }
}