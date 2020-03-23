using Microsoft.EntityFrameworkCore;
using UdmApi.Proxy.Data.Models;

namespace UdmApi.Proxy.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<SsoSession> Sessions { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
    }
}