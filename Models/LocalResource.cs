namespace JASS.Bookmark.Master.Models;

public class LocalResource
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string Type { get; set; } = "File";
    public string Extension { get; set; } = "";
    public long Size { get; set; }
    public DateTime Modified { get; set; }
    public string Folder { get; set; } = "";
    public string SizeText => Size < 1024 ? $"{Size:N0} B" : Size < 1024 * 1024 ? $"{Size / 1024d:N1} KB" : Size < 1024 * 1024 * 1024 ? $"{Size / 1024d / 1024d:N1} MB" : $"{Size / 1024d / 1024d / 1024d:N1} GB";
}
