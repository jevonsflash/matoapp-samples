using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace MatoAppSample.EntityFrameworkCore
{
    public static class MatoAppSampleDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<MatoAppSampleDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<MatoAppSampleDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
