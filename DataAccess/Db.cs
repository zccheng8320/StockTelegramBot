using System;
using DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class Db :DbContext
    {
        public Db(DbContextOptions<Db> options):base(options)
        {
            
        }
        public DbSet<UserPortfolio> UserPortfolios { get; set; }
    }
}