using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using BookmarkModel = JASS.Bookmark.Master.Models.Bookmark;
using JASS.Bookmark.Master.Models;
using JASS.Bookmark.Master.Services;

namespace JASS.Bookmark.Master;

public partial class LocalResources : UserControl
{
    private readonly LocalResourceService scanner = new();
    private readonly BookmarkDbService db = new();
    private List<LocalResource> results = new();

    public LocalResources() { InitializeComponent(); }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose a local folder to search", Multiselect = false };
        if (dialog.ShowDialog() == true) { FolderBox.Text = dialog.FolderName; Scan(); }
    }

    private void Scan_Click(object sender, RoutedEventArgs e) => Scan();

    private void Scan()
    {
        if (!System.IO.Directory.Exists(FolderBox.Text))
        {
            MessageBox.Show("Choose an existing local folder first.", "Local Resources", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        try
        {
            results = scanner.Scan(FolderBox.Text, LocalSearchBox.Text, IncludeFolders.IsChecked == true);
            LocalGrid.ItemsSource = results;
            ResultSummary.Text = $"{results.Count:N0} resources found • double-click to open • select one or more to add as bookmarks";
        }
        finally { Mouse.OverrideCursor = null; }
    }

    private void LocalSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded && System.IO.Directory.Exists(FolderBox.Text)) Scan();
    }

    private void AddSelected_Click(object sender, RoutedEventArgs e)
    {
        AddResources(LocalGrid.SelectedItems.Cast<LocalResource>());
    }

    private void AddAll_Click(object sender, RoutedEventArgs e)
    {
        AddResources(results);
    }

    private void AddResources(IEnumerable<LocalResource> resources)
    {
        var items = resources.ToList();
        if (items.Count == 0) { MessageBox.Show("Select at least one local resource.", "Local Resources", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        int added = 0;
        foreach (var item in items)
        {
            var bookmark = new BookmarkModel
            {
                Title = item.Name,
                Url = LocalResourceService.ToFileUri(item.FullPath),
                Folder = "Local Resources",
                Tags = item.Type.Equals("Folder", StringComparison.OrdinalIgnoreCase) ? "local,folder" : $"local,{item.Extension.TrimStart('.').ToLowerInvariant()}",
                Description = item.FullPath,
                Notes = $"Local resource added from {FolderBox.Text}",
                Source = "Local Folder"
            };
            if (db.Add(bookmark)) added++;
        }
        MessageBox.Show($"Added {added:N0} new local links. Existing links were skipped.", "Local Resources", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LocalGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (LocalGrid.SelectedItem is not LocalResource item) return;
        try { Process.Start(new ProcessStartInfo(item.FullPath) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"Could not open the resource.\n\n{ex.Message}", "Local Resources", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
