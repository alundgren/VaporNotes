using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests
{
    internal class FakeDropboxService : IDropboxService
    {
        private ConcurrentDictionary<string, byte[]> filesByPath = new ConcurrentDictionary<string, byte[]>();

        public Task DeleteFilesAsync(List<DropboxFileReference> ids)
        {
            ids.ForEach(id => filesByPath.Remove(id.Path, out var _));
            return Task.CompletedTask;
        }

        public Task<Stream?> LoadFileAsync(DropboxFileReference file)
        {
            if (filesByPath.TryGetValue(file.Path, out var value))
                return Task.FromResult((Stream?)new MemoryStream(value));
            else
                return Task.FromResult((Stream?)null);
        }

        public async Task SaveFileAsync(Stream content, DropboxFileReference file)
        {
            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            filesByPath[file.Path] = ms.ToArray();
        }
    }
}
