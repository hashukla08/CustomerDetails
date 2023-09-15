using CustomerDetails.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerDetails.API.DataAccess.Data
{
    public class ApplicationDBContext : DbContext
    {

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<Customer> Customers { get; set; }

    }
}
