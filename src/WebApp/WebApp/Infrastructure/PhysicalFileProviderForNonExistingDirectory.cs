using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;

namespace WebApp.Infrastructure
{
    public class PhysicalFileProviderForNonExistingDirectory : IFileProvider
    {
        private string root;
        private PhysicalFileProvider physicalFileProvider;

        public PhysicalFileProviderForNonExistingDirectory(string root)
        {
            this.root = root;
        }

        private PhysicalFileProvider GetPhysicalFileProvider()
        {
            if (!File.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            if (physicalFileProvider == null)
            {
                physicalFileProvider = new PhysicalFileProvider(root);
            }
            return physicalFileProvider;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return GetPhysicalFileProvider().GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return GetPhysicalFileProvider().GetFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return GetPhysicalFileProvider().Watch(filter);
        }
    }
}
