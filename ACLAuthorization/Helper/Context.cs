using ACLAuthorization.Models;
using Microsoft.EntityFrameworkCore;

namespace ACLAuthorization.Helper
{
    public class Context : DbContext
    {
        public DbSet<User> users { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<Permission> permissions { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
        }

    }
}
