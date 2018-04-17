using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace BaGet.Core.Services
{
    public class PackageService : IPackageService
    {
        private readonly IContext _context;

        public PackageService(IContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PackageAddResult> AddAsync(Package package)
        {
            try
            {
                _context.Packages.Add(package);

                await _context.SaveChangesAsync();

                return PackageAddResult.Success;
            }
            catch (DbUpdateException e)
                when (_context.IsUniqueConstraintViolationException(e))
            {
                return PackageAddResult.PackageAlreadyExists;
            }
        }

        public Task<bool> ExistsAsync(string id, NuGetVersion version)
            => _context.Packages
                .Where(p => p.Id == id)
                .Where(p => p.VersionString == version.ToNormalizedString())
                .AnyAsync();

        public async Task<IReadOnlyList<Package>> FindAsync(string id)
        {
            var results = await _context.Packages
                .Where(p => p.Id == id)
                .ToListAsync();

            return results.AsReadOnly();
        }

        public Task<Package> FindAsync(string id, NuGetVersion version)
            => _context.Packages
                .Where(p => p.Id == id)
                .Where(p => p.VersionString == version.ToNormalizedString())
                .FirstOrDefaultAsync();

        public Task<bool> UnlistPackageAsync(string id, NuGetVersion version)
        {
            return RemovePackageAsync(id, version);
            //return TryUpdatePackageAsync(id, version, p => p.Listed = false);
        }

        public Task<bool> RelistPackageAsync(string id, NuGetVersion version)
        {
            return TryUpdatePackageAsync(id, version, p => p.Listed = true);
        }

        private async Task<bool> RemovePackageAsync(string id, NuGetVersion version)
        {
            bool result = false;

            var package = await FindAsync(id, version);

            if (package != null)
            {
                Console.Write("Package found!");

                _context.Packages.Remove(package);

                result = await _context.SaveChangesAsync() > 0;
            }
            else
            {
                Console.Write("Package not found!");
            }

            Console.Write($"Result value: {result.ToString()}");

            return result;
        }

        private async Task<bool> TryUpdatePackageAsync(string id, NuGetVersion version, Action<Package> action)
        {
            bool result = false;

            var package = await FindAsync(id, version);

            if (package != null)
            {
                action(package);

                result = await _context.SaveChangesAsync() > 0;
            }

            return result;
        }

        public async Task AddDownloadAsync(string id, NuGetVersion version)
        {
            var package = await FindAsync(id, version);

            package.Downloads += 1;

            await _context.SaveChangesAsync();
        }
    }
}