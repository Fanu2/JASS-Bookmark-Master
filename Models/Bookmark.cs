namespace JASS.Bookmark.Master.Models;
public class Bookmark
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string Folder { get; set; } = "Unsorted";
    public string Tags { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = "Manual";
    public string ResourceType { get; set; } = "Web Page";
    public string DateAdded { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public string LastVisited { get; set; } = "";
    public bool Favorite { get; set; }
    public bool ReadLater { get; set; }
    public bool Archived { get; set; }
    public bool Broken { get; set; }
    public string Domain => Uri.TryCreate(Url, UriKind.Absolute, out var u) ? u.Host : "";
}
