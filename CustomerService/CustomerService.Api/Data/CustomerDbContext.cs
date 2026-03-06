using Microsoft.EntityFrameworkCore;
using CustomerService.Api.Models;

namespace CustomerService.Api.Data;

//Bridge class between c# and the DB (sqlite)
public class CustomerDbContext : DbContext //DbContext is a base class from EF Core. handle Db connections
{
    //base constructor
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options) { }

    //DbSet represents a collection of entities in the context, and it corresponds to a table in the database. It provides methods for querying and saving instances of the entity type.
    public DbSet<Customer> Customers => Set<Customer>();
}
