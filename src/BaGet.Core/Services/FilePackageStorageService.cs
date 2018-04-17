using System;
using System.IO;
using System.Threading.Tasks;
using BaGet.Core.Extensions;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace BaGet.Core.Services
{
    public class FilePackageStorageService : IPackageStorageService
    {
        private readonly string _storePath;

        public FilePackageStorageService(string storePath)
        {
            _storePath = storePath ?? throw new ArgumentNullException(nameof(storePath));
        }

        public async Task SavePackageStreamAsync(PackageArchiveReader package, Stream packageStream)
        {
            var identity = package.GetIdentity();
            var packagePath = Path.Combine(_storePath, identity.PackagePath());
            var nuspecPath = Path.Combine(_storePath, identity.NuspecPath());
            var readmePath = Path.Combine(_storePath, identity.ReadmePath());

            EnsurePathExists(identity);

            // TODO: Catch IOException and test if File.Exists. If false, rethrow exception.
            using (var fileStream = File.Open(packagePath, FileMode.CreateNew))
            {
                packageStream.Seek(0, SeekOrigin.Begin);

                await packageStream.CopyToAsync(fileStream);
            }

            using (var nuspec = package.GetNuspec())
            using (var fileStream = File.Open(nuspecPath, FileMode.CreateNew))
            {
                await nuspec.CopyToAsync(fileStream);
            }

            using (var readme = package.GetReadme())
            using (var fileStream = File.Open(readmePath, FileMode.CreateNew))
            {
                await readme.CopyToAsync(fileStream);
            }
        }

        public Task<Stream> GetPackageStreamAsync(PackageIdentity package) => Task.FromResult(GetPackageStream(package));
        public Task<Stream> GetNuspecStreamAsync(PackageIdentity package) => Task.FromResult(GetNuspecStream(package));
        public Task<Stream> GetReadmeStreamAsync(PackageIdentity package) => Task.FromResult(GetReadmeStream(package));

        private Stream GetPackageStream(PackageIdentity package) => GetFileStream(package.PackagePath());
        private Stream GetNuspecStream(PackageIdentity package) => GetFileStream(package.NuspecPath());
        private Stream GetReadmeStream(PackageIdentity package) => GetFileStream(package.ReadmePath());

        private Stream GetFileStream(string path)
        {
            path = Path.Combine(_storePath, path);

            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public async Task<bool> DeleteAsync(PackageIdentity package)
        {
            bool result = false;

            try
            {
                var packagePath = Path.Combine(_storePath, package.ReadmePath());
                var nuspecPath = Path.Combine(_storePath, package.NuspecPath());
                var readmePath = Path.Combine(_storePath, package.ReadmePath());

                Console.WriteLine($"Package path: {packagePath}");
                Console.WriteLine($"Nuspec path: {nuspecPath}");
                Console.WriteLine($"Readme path: {readmePath}");

                await Task.Run(() =>
                {
                    DirectoryInfo di = new DirectoryInfo(packagePath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        Console.WriteLine($"Delete file: {file.Name}");

                        file.Delete();
                    }

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        Console.WriteLine($"Delete folder: {dir.Name}");

                        dir.Delete(true);
                    }
                });

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }

        private void EnsurePathExists(PackageIdentity package)
        {
            var id = package.Id;
            var version = package.Version.ToNormalizedString();
            var path = Path.Combine(_storePath, id, version);

            Directory.CreateDirectory(path);
        }
    }
}
