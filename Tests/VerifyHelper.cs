using System.IO.Compression;

namespace Tests
{
    internal static class VerifyHelper
    {
        public static async Task Verify(string path, VerifySettings? settings = null)
        {
            try
            {
                using var archive = ZipFile.Open(path, ZipArchiveMode.Read);
                var descriptions = new List<object>();
                foreach (var entry in archive.Entries)
                {
                    using var stream = entry.Open();
                    using var streamReader = new StreamReader(stream);
                    var content = await streamReader.ReadToEndAsync();
                    content = content.Replace("><", ">\n<");
                    descriptions.Add(new
                    {
                        Path = entry.Name,
                        Content = content,
                    });
                }
                await Verifier.Verify(descriptions, settings);
            }
            finally
            {
                try
                {
                    File.Delete(path);
                }
                catch { }
            }
        }
    }
}
