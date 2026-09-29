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
    private void Initialize(){
        using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand();
        cmd.CommandText="""
CREATE TABLE IF NOT EXISTS Bookmarks(Id INTEGER PRIMARY KEY AUTOINCREMENT,Title TEXT NOT NULL,Url TEXT NOT NULL UNIQUE,Folder TEXT,Tags TEXT,Notes TEXT,Description TEXT,Source TEXT,ResourceType TEXT,DateAdded TEXT,LastVisited TEXT,Favorite INTEGER,ReadLater INTEGER,Archived INTEGER,Broken INTEGER);
CREATE INDEX IF NOT EXISTS IX_Bookmarks_Folder ON Bookmarks(Folder); CREATE INDEX IF NOT EXISTS IX_Bookmarks_Url ON Bookmarks(Url); CREATE INDEX IF NOT EXISTS IX_Bookmarks_ResourceType ON Bookmarks(ResourceType);
CREATE TABLE IF NOT EXISTS Collections(Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL UNIQUE,Description TEXT);
CREATE TABLE IF NOT EXISTS CollectionItems(CollectionId INTEGER NOT NULL,BookmarkId INTEGER NOT NULL,PRIMARY KEY(CollectionId,BookmarkId),FOREIGN KEY(CollectionId) REFERENCES Collections(Id) ON DELETE CASCADE,FOREIGN KEY(BookmarkId) REFERENCES Bookmarks(Id) ON DELETE CASCADE);
"""; cmd.ExecuteNonQuery();
        using var check=c.CreateCommand(); check.CommandText="PRAGMA table_info(Bookmarks)"; using var r=check.ExecuteReader(); var hasType=false; while(r.Read()) if(r.GetString(1).Equals("ResourceType",StringComparison.OrdinalIgnoreCase)) hasType=true; r.Close();
        if(!hasType){using var alter=c.CreateCommand(); alter.CommandText="ALTER TABLE Bookmarks ADD COLUMN ResourceType TEXT DEFAULT 'Web Page'"; alter.ExecuteNonQuery();}
        NormalizeLocalResourceTypes(c);
    }
    private static void NormalizeLocalResourceTypes(SqliteConnection c)
    {
        using var read=c.CreateCommand();
        read.CommandText="SELECT Id,Url,Folder,Tags,Source,ResourceType FROM Bookmarks";
        using var r=read.ExecuteReader();
        var updates=new List<(long Id,string Type)>();
        while(r.Read())
        {
            var id=r.GetInt64(0);
            var url=r.IsDBNull(1)?"":r.GetString(1);
            var folder=r.IsDBNull(2)?"":r.GetString(2);
            var tags=r.IsDBNull(3)?"":r.GetString(3);
            var source=r.IsDBNull(4)?"":r.GetString(4);
            var type=r.IsDBNull(5)?"":r.GetString(5);
            if(!url.StartsWith("file:///",StringComparison.OrdinalIgnoreCase)) continue;
            if(!type.Equals("Web Page",StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(type)) continue;
            var localType = tags.Contains("local,folder",StringComparison.OrdinalIgnoreCase) ? "Local Folder" : "Local File";
            try
            {
                if(Uri.TryCreate(url,UriKind.Absolute,out var uri))
                {
                    var path=uri.LocalPath;
                    if(Directory.Exists(path)) localType="Local Folder";
                    else if(File.Exists(path)) localType="Local File";
                }
            }
            catch { }
            if(folder.Equals("Local Resources",StringComparison.OrdinalIgnoreCase) ||
               source.Equals("Local Folder",StringComparison.OrdinalIgnoreCase) ||
               tags.Contains("local",StringComparison.OrdinalIgnoreCase))
                updates.Add((id,localType));
        }
        r.Close();
        foreach(var item in updates)
        {
            using var update=c.CreateCommand();
            update.CommandText="UPDATE Bookmarks SET ResourceType=$rt WHERE Id=$id";
            update.Parameters.AddWithValue("$rt",item.Type);
            update.Parameters.AddWithValue("$id",item.Id);
            update.ExecuteNonQuery();
        }
    }
    public List<BookmarkModel> GetAll(){ var list=new List<BookmarkModel>(); using var c=new SqliteConnection(C); c.Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT Id,Title,Url,Folder,Tags,Notes,Description,Source,ResourceType,DateAdded,LastVisited,Favorite,ReadLater,Archived,Broken FROM Bookmarks ORDER BY Favorite DESC, Title COLLATE NOCASE"; using var r=cmd.ExecuteReader(); while(r.Read()) list.Add(Read(r)); return list; }
    private static BookmarkModel Read(SqliteDataReader r)=>new(){Id=r.GetInt64(0),Title=r.GetString(1),Url=r.GetString(2),Folder=r.IsDBNull(3)?"Unsorted":r.GetString(3),Tags=r.IsDBNull(4)?"":r.GetString(4),Notes=r.IsDBNull(5)?"":r.GetString(5),Description=r.IsDBNull(6)?"":r.GetString(6),Source=r.IsDBNull(7)?"Manual":r.GetString(7),ResourceType=r.IsDBNull(8)?"Web Page":r.GetString(8),DateAdded=r.IsDBNull(9)?"":r.GetString(9),LastVisited=r.IsDBNull(10)?"":r.GetString(10),Favorite=r.GetInt32(11)!=0,ReadLater=r.GetInt32(12)!=0,Archived=r.GetInt32(13)!=0,Broken=r.GetInt32(14)!=0};
    private static void AddParams(SqliteCommand c,BookmarkModel b){c.Parameters.AddWithValue("$t",b.Title);c.Parameters.AddWithValue("$u",b.Url);c.Parameters.AddWithValue("$f",b.Folder);c.Parameters.AddWithValue("$g",b.Tags);c.Parameters.AddWithValue("$n",b.Notes);c.Parameters.AddWithValue("$d",b.Description);c.Parameters.AddWithValue("$s",b.Source);c.Parameters.AddWithValue("$rt",b.ResourceType);c.Parameters.AddWithValue("$a",b.DateAdded);c.Parameters.AddWithValue("$v",b.LastVisited);c.Parameters.AddWithValue("$fav",b.Favorite?1:0);c.Parameters.AddWithValue("$rl",b.ReadLater?1:0);c.Parameters.AddWithValue("$ar",b.Archived?1:0);c.Parameters.AddWithValue("$br",b.Broken?1:0);}
    public bool Add(BookmarkModel b){try{using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="INSERT INTO Bookmarks(Title,Url,Folder,Tags,Notes,Description,Source,ResourceType,DateAdded,LastVisited,Favorite,ReadLater,Archived,Broken) VALUES($t,$u,$f,$g,$n,$d,$s,$rt,$a,$v,$fav,$rl,$ar,$br)";AddParams(cmd,b);cmd.ExecuteNonQuery();return true;}catch(SqliteException){return false;}}
    public void Update(BookmarkModel b){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="UPDATE Bookmarks SET Title=$t,Url=$u,Folder=$f,Tags=$g,Notes=$n,Description=$d,Source=$s,ResourceType=$rt,Favorite=$fav,ReadLater=$rl,Archived=$ar,Broken=$br WHERE Id=$id";AddParams(cmd,b);cmd.Parameters.AddWithValue("$id",b.Id);cmd.ExecuteNonQuery();}
    public void Delete(long id){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM Bookmarks WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();}
    public void MarkBroken(long id,bool broken){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="UPDATE Bookmarks SET Broken=$b WHERE Id=$id";cmd.Parameters.AddWithValue("$b",broken?1:0);cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();}
    public BookmarkStats Stats(){var items=GetAll();var folders=items.Select(x=>string.IsNullOrWhiteSpace(x.Folder)?"Unsorted":x.Folder).Distinct(StringComparer.OrdinalIgnoreCase).Count();var domains=items.Select(x=>x.Domain).Where(x=>!string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Count();return new(items.Count,items.Count(x=>x.Favorite),items.Count(x=>x.ReadLater),items.Count(x=>x.Archived),items.Count(x=>x.Broken),folders,domains);}
    public List<ResourceCollection> GetCollections(){var list=new List<ResourceCollection>();using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT c.Id,c.Name,c.Description,COUNT(ci.BookmarkId) FROM Collections c LEFT JOIN CollectionItems ci ON ci.CollectionId=c.Id GROUP BY c.Id,c.Name,c.Description ORDER BY c.Name COLLATE NOCASE";using var r=cmd.ExecuteReader();while(r.Read())list.Add(new(){Id=r.GetInt64(0),Name=r.GetString(1),Description=r.IsDBNull(2)?"":r.GetString(2),ResourceCount=r.GetInt32(3)});return list;}
    public long AddCollection(string name,string description){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="INSERT INTO Collections(Name,Description) VALUES($n,$d); SELECT last_insert_rowid();";cmd.Parameters.AddWithValue("$n",name.Trim());cmd.Parameters.AddWithValue("$d",description.Trim());return (long)cmd.ExecuteScalar()!;}
    public void DeleteCollection(long id){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM Collections WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();}
    public void AddToCollection(long collectionId,long bookmarkId){using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="INSERT OR IGNORE INTO CollectionItems(CollectionId,BookmarkId) VALUES($c,$b)";cmd.Parameters.AddWithValue("$c",collectionId);cmd.Parameters.AddWithValue("$b",bookmarkId);cmd.ExecuteNonQuery();}
    public List<long> GetCollectionBookmarkIds(long collectionId){var list=new List<long>();using var c=new SqliteConnection(C);c.Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT BookmarkId FROM CollectionItems WHERE CollectionId=$c";cmd.Parameters.AddWithValue("$c",collectionId);using var r=cmd.ExecuteReader();while(r.Read())list.Add(r.GetInt64(0));return list;}
}
