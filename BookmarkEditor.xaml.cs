using System.Windows; using BookmarkModel = JASS.Bookmark.Master.Models.Bookmark;
using JASS.Bookmark.Master.Models;
namespace JASS.Bookmark.Master;
public partial class BookmarkEditor:Window
{ public BookmarkModel Bookmark{get;private set;}=new(); public BookmarkEditor(){InitializeComponent();Owner=Application.Current.MainWindow;} void Save_Click(object s,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(Url.Text)||!Uri.TryCreate(Url.Text.Trim(),UriKind.Absolute,out _)){MessageBox.Show("Enter a valid URL.");return;}Bookmark.Title=string.IsNullOrWhiteSpace(TitleBox.Text)?Url.Text.Trim():TitleBox.Text.Trim();Bookmark.Url=Url.Text.Trim();Bookmark.Folder=string.IsNullOrWhiteSpace(Folder.Text)?"Unsorted":Folder.Text.Trim();Bookmark.Tags=Tags.Text.Trim();Bookmark.Description=Description.Text.Trim();Bookmark.Notes=Notes.Text.Trim();DialogResult=true;}void Cancel_Click(object s,RoutedEventArgs e){DialogResult=false;}}
