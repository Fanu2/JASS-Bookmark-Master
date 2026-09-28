using System.IO;
using JASS.Bookmark.Master.Models;

namespace JASS.Bookmark.Master.Services;

public class LocalResourceService
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "$Recycle.Bin", "System Volume Information", "Windows", "node_modules", ".git", "bin", "obj"
    };

    public List<LocalResource> Scan(string root, string query = "", bool includeDirectories = false, int maxResults = 20000)
    {
        var results = new List<LocalResource>();
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return results;
        var normalizedQuery = query.Trim();
        try
        {
            ScanDirectory(root, normalizedQuery, includeDirectories, results, maxResults);
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
        return results.OrderBy(x => x.Type).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private void ScanDirectory(string directory, string query, bool includeDirectories, List<LocalResource> results, int maxResults)
    {
        if (results.Count >= maxResults) return;
        DirectoryInfo info;
        try { info = new DirectoryInfo(directory); }
        catch { return; }

        if (includeDirectories && Matches(info.Name, info.FullName, query))
        {
            results.Add(new LocalResource { Name = info.Name, FullPath = info.FullName, Type = "Folder", Folder = info.Parent?.FullName ?? info.FullName, Modified = info.LastWriteTime });
            if (results.Count >= maxResults) return;
        }

        IEnumerable<FileInfo> files;
        try { files = info.EnumerateFiles(); } catch { files = Array.Empty<FileInfo>(); }
        foreach (var file in files)
        {
            if (results.Count >= maxResults) break;
            if (!Matches(file.Name, file.FullName, query)) continue;
            try
            {
                results.Add(new LocalResource
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    Type = "File",
                    Extension = file.Extension,
                    Size = file.Length,
                    Modified = file.LastWriteTime,
                    Folder = file.DirectoryName ?? ""
                });
            }
            catch { }
        }

        IEnumerable<DirectoryInfo> children;
        try { children = info.EnumerateDirectories(); } catch { children = Array.Empty<DirectoryInfo>(); }
        foreach (var child in children)
        {
            if (results.Count >= maxResults) break;
            if (IgnoredDirectories.Contains(child.Name)) continue;
            ScanDirectory(child.FullName, query, includeDirectories, results, maxResults);
        }
    }

    private static bool Matches(string name, string path, string query) =>
        string.IsNullOrWhiteSpace(query) || name.Contains(query, StringComparison.OrdinalIgnoreCase) || path.Contains(query, StringComparison.OrdinalIgnoreCase);

    public static string ToFileUri(string path) => new Uri(Path.GetFullPath(path)).AbsoluteUri;
}
