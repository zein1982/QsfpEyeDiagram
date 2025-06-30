using Microsoft.EntityFrameworkCore;
using QSFP_eye_auto.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSFP_eye_auto.Models
{
    public class DContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseMySql("server=10.0.30.105;database=production;uid=commonUser;pwd=commonUser3000!"/*, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.20-mysql")*/);//Pomelo
                optionsBuilder.UseMySQL("Server = 10.0.30.105;Database = production;UserId = commonUser;Password = commonUser3000!;SslMode=None");
            }
        }

        public DbSet<Worker> workers { get; set; }
        public DbSet<TuneLimits> QsfpLimits { get; set; }
    }
}
