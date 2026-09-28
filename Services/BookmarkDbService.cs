using Microsoft.Data.Sqlite;
using BookmarkModel = JASS.Bookmark.Master.Models.Bookmark;
using JASS.Bookmark.Master.Models;
using System.IO;
namespace JASS.Bookmark.Master.Services;
public class BookmarkDbService
{
    private readonly string _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JASS", "BookmarkMaster", "bookmarks.db");
    private string C => $"Data Source={_dbPath}";
    public BookmarkDbService(){ Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!); Initialize(); }
    private void Initialize(){ using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="""
CREATE TABLE IF NOT EXISTS Bookmarks(Id INTEGER PRIMARY KEY AUTOINCREMENT,Title TEXT NOT NULL,Url TEXT NOT NULL UNIQUE,Folder TEXT,Tags TEXT,Notes TEXT,Description TEXT,Source TEXT,DateAdded TEXT,LastVisited TEXT,Favorite INTEGER,ReadLater INTEGER,Archived INTEGER,Broken INTEGER);
CREATE INDEX IF NOT EXISTS IX_Bookmarks_Folder ON Bookmarks(Folder); CREATE INDEX IF NOT EXISTS IX_Bookmarks_Url ON Bookmarks(Url);
"""; cmd.ExecuteNonQuery(); }
    public List<BookmarkModel> GetAll(){ var list=new List<BookmarkModel>(); using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT * FROM Bookmarks ORDER BY Favorite DESC, Title COLLATE NOCASE"; using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(Read(r)); return list; }
    private static BookmarkModel Read(SqliteDataReader r)=>new BookmarkModel(){Id=r.GetInt64(0),Title=r.GetString(1),Url=r.GetString(2),Folder=r.IsDBNull(3)?"Unsorted":r.GetString(3),Tags=r.IsDBNull(4)?"":r.GetString(4),Notes=r.IsDBNull(5)?"":r.GetString(5),Description=r.IsDBNull(6)?"":r.GetString(6),Source=r.IsDBNull(7)?"Manual":r.GetString(7),DateAdded=r.IsDBNull(8)?"":r.GetString(8),LastVisited=r.IsDBNull(9)?"":r.GetString(9),Favorite=r.GetInt32(10)!=0,ReadLater=r.GetInt32(11)!=0,Archived=r.GetInt32(12)!=0,Broken=r.GetInt32(13)!=0};
    public bool Add(BookmarkModel b){ try{ using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="INSERT INTO Bookmarks(Title,Url,Folder,Tags,Notes,Description,Source,DateAdded,LastVisited,Favorite,ReadLater,Archived,Broken) VALUES($t,$u,$f,$g,$n,$d,$s,$a,$v,$fav,$rl,$ar,$br)"; AddParams(cmd,b); cmd.ExecuteNonQuery(); return true;}catch(SqliteException){return false;} }
    public void Upsert(BookmarkModel b){ using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="INSERT INTO Bookmarks(Title,Url,Folder,Tags,Notes,Description,Source,DateAdded,LastVisited,Favorite,ReadLater,Archived,Broken) VALUES($t,$u,$f,$g,$n,$d,$s,$a,$v,$fav,$rl,$ar,$br) ON CONFLICT(Url) DO UPDATE SET Title=excluded.Title,Folder=excluded.Folder,Tags=excluded.Tags,Notes=excluded.Notes,Description=excluded.Description,Source=excluded.Source,Favorite=excluded.Favorite,ReadLater=excluded.ReadLater,Archived=excluded.Archived,Broken=excluded.Broken"; AddParams(cmd,b); cmd.ExecuteNonQuery(); }
    private static void AddParams(SqliteCommand c,BookmarkModel b){c.Parameters.AddWithValue("$t",b.Title);c.Parameters.AddWithValue("$u",b.Url);c.Parameters.AddWithValue("$f",b.Folder);c.Parameters.AddWithValue("$g",b.Tags);c.Parameters.AddWithValue("$n",b.Notes);c.Parameters.AddWithValue("$d",b.Description);c.Parameters.AddWithValue("$s",b.Source);c.Parameters.AddWithValue("$a",b.DateAdded);c.Parameters.AddWithValue("$v",b.LastVisited);c.Parameters.AddWithValue("$fav",b.Favorite?1:0);c.Parameters.AddWithValue("$rl",b.ReadLater?1:0);c.Parameters.AddWithValue("$ar",b.Archived?1:0);c.Parameters.AddWithValue("$br",b.Broken?1:0);}
    public void Update(BookmarkModel b){ using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="UPDATE Bookmarks SET Title=$t,Url=$u,Folder=$f,Tags=$g,Notes=$n,Description=$d,Favorite=$fav,ReadLater=$rl,Archived=$ar,Broken=$br WHERE Id=$id"; AddParams(cmd,b); cmd.Parameters.AddWithValue("$id",b.Id); cmd.ExecuteNonQuery(); }
    public void Delete(long id){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM Bookmarks WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();}
    public void MarkBroken(long id,bool broken){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="UPDATE Bookmarks SET Broken=$b WHERE Id=$id";cmd.Parameters.AddWithValue("$b",broken?1:0);cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();}
    public BookmarkStats Stats()
    {
        var items = GetAll();
        var folders = items.Select(x => string.IsNullOrWhiteSpace(x.Folder) ? "Unsorted" : x.Folder).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var domains = items.Select(x => x.Domain).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        return new BookmarkStats(
            items.Count,
            items.Count(x => x.Favorite),
            items.Count(x => x.ReadLater),
            items.Count(x => x.Archived),
            items.Count(x => x.Broken),
            folders,
            domains);
    }
}
