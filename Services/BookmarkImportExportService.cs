using System.IO;
using System.Text.Json; using System.Text; using System.Net; using BookmarkModel = JASS.Bookmark.Master.Models.Bookmark;
using JASS.Bookmark.Master.Models;
namespace JASS.Bookmark.Master.Services;
public class BookmarkImportExportService
{
 public List<BookmarkModel> ImportHtml(string path,string source){var html=File.ReadAllText(path);var list=new List<BookmarkModel>();foreach(System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(html,"<a\\s+[^>]*href=[\\\"']([^\\\"']+)[\\\"'][^>]*>(.*?)</a>",System.Text.RegularExpressions.RegexOptions.IgnoreCase|System.Text.RegularExpressions.RegexOptions.Singleline)){var u=WebUtility.HtmlDecode(m.Groups[1].Value).Trim();var t=WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(m.Groups[2].Value,"<.*?>"," ")).Trim();if(Uri.TryCreate(u,UriKind.Absolute,out _))list.Add(new BookmarkModel{Title=string.IsNullOrWhiteSpace(t)?u:t,Url=u,Source=source,DateAdded=DateTime.Now.ToString("yyyy-MM-dd HH:mm")});}return list;}
 public void ExportJson(string path,IEnumerable<BookmarkModel> b)=>File.WriteAllText(path,JsonSerializer.Serialize(b,new JsonSerializerOptions{WriteIndented=true}));
 public void ExportHtml(string path,IEnumerable<BookmarkModel> bookmarks){var sb=new StringBuilder("<!doctype html><html><head><meta charset='utf-8'><title>JASS Bookmark Master</title></head><body><h1>JASS Bookmark Master</h1><ul>");foreach(var b in bookmarks)sb.Append($"<li><a href=\"{WebUtility.HtmlEncode(b.Url)}\">{WebUtility.HtmlEncode(b.Title)}</a> <small>[{WebUtility.HtmlEncode(b.Folder)}]</small></li>");sb.Append("</ul></body></html>");File.WriteAllText(path,sb.ToString());}
 public List<BookmarkModel> ImportJson(string path){var x=JsonSerializer.Deserialize<List<BookmarkModel>>(File.ReadAllText(path));return x??new();}
}
