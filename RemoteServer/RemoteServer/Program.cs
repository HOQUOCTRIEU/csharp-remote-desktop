using RemoteServer.Models;
using System.Text;

namespace RemoteServer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                if (!context.Users.Any())
                {
                    var user = new User { Username = "client1", PasswordHash = HashPassword("password123") };
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            }

            string HashPassword(string password)
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
            Application.Run(new Form1());
        }
    }
}