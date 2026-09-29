using System.Windows; using BookmarkModel=JASS.Bookmark.Master.Models.Bookmark;
namespace JASS.Bookmark.Master;
public partial class BookmarkEditor:Window
{
 public BookmarkModel Bookmark{get;private set;}=new();
 static readonly string[] Types={"Web Page","GitHub Repository","Documentation","PDF","Dataset","Software","Video","Image","Local File","Local Folder","Other"};
 public BookmarkEditor(BookmarkModel? existing=null){InitializeComponent();Owner=Application.Current.MainWindow;TypeBox.ItemsSource=Types;Bookmark=existing??new BookmarkModel();Header.Text=existing==null?"Add Resource":"Edit Resource";Title=Header.Text;if(existing!=null){TitleBox.Text=existing.Title;Url.Text=existing.Url;Folder.Text=existing.Folder;Tags.Text=existing.Tags;Description.Text=existing.Description;Notes.Text=existing.Notes;TypeBox.SelectedItem=existing.ResourceType;}else TypeBox.SelectedIndex=0;}
 void Save_Click(object s,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(Url.Text)||!Uri.TryCreate(Url.Text.Trim(),UriKind.Absolute,out _)){MessageBox.Show("Enter a valid URL or file:/// path.","Resource Library",MessageBoxButton.OK,MessageBoxImage.Information);return;}Bookmark.Title=string.IsNullOrWhiteSpace(TitleBox.Text)?Url.Text.Trim():TitleBox.Text.Trim();Bookmark.Url=Url.Text.Trim();Bookmark.Folder=string.IsNullOrWhiteSpace(Folder.Text)?"Unsorted":Folder.Text.Trim().Trim('/');Bookmark.Tags=Tags.Text.Trim();Bookmark.Description=Description.Text.Trim();Bookmark.Notes=Notes.Text.Trim();Bookmark.ResourceType=TypeBox.SelectedItem?.ToString()??"Other";if(string.IsNullOrWhiteSpace(Bookmark.DateAdded))Bookmark.DateAdded=DateTime.Now.ToString("yyyy-MM-dd HH:mm");DialogResult=true;}
 void Cancel_Click(object s,RoutedEventArgs e){DialogResult=false;}
}
