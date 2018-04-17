﻿using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BaGet.Core.Entities
{
    public interface IContext
    {
        DatabaseFacade Database { get; }

        DbSet<Package> Packages { get; set; }

        DbSet<PackageDependency> PackageDependencies { get; set; }

        DbSet<PackageDependencyGroup> PackageDependencyGroups { get; set; }

        /// <summary>
        /// Check whether a <see cref="DbUpdateException"/> is due to a SQL unique constraint violation.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns>Whether the exception was caused to SQL unique constraint violation.</returns>
        bool IsUniqueConstraintViolationException(DbUpdateException exception);

        Task<int> SaveChangesAsync();
    }
}
