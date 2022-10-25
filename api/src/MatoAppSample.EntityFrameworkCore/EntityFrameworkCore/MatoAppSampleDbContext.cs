using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using MatoAppSample.Authorization.Roles;
using MatoAppSample.Authorization.Users;
using MatoAppSample.MultiTenancy;

namespace MatoAppSample.EntityFrameworkCore
{
    public class MatoAppSampleDbContext : AbpZeroDbContext<Tenant, Role, User, MatoAppSampleDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public MatoAppSampleDbContext(DbContextOptions<MatoAppSampleDbContext> options)
            : base(options)
        {
        }
    }
}
