using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace AdminSysAPI.Models
{
    public class dBContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<OTPLog> OTPLog { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=sql.bsite.net\\MSSQL2016;Database=skanolkar_;User Id=skanolkar_;password=1234;TrustServerCertificate=true");

            //optionsBuilder.UseSqlServer("Server=SUJAY;Database=adminSystem;Integrated Security=true;TrustServerCertificate=true");
        }
    }
}
