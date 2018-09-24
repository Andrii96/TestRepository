using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Devabit.Telelingua.ReportingServices.DataAccess
{
   public  class QueryDbContext : DbContext
    {

        public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options) { }

        public DbSet<Query> Queries { get; set; }
        public DbSet<ErrorQuery> Errors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SqlScript> SqlScripts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Query>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<ErrorQuery>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Category>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<SqlScript>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Category>().HasMany(x => x.Queries).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId);
            builder.Entity<Category>().HasMany(x => x.SqlScripts).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId);
        }
    }
}
