﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Detached.EntityFramework.Tests.Plugins.Pagination
{
    public class PaginationContext : DbContext
    {
        public DbSet<PaginationEntity> Entities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var serviceProvider = new ServiceCollection()
                                         .AddEntityFrameworkInMemoryDatabase()
                                         .AddDetachedEntityFramework()
                                         .BuildServiceProvider();

            optionsBuilder.UseInternalServiceProvider(serviceProvider)
                          .UseInMemoryDatabase("test")
                          .UseDetached();
        }
    }
}
