using System.IO;
using System.Windows;

namespace JASS.Bookmark.Master;

public partial class App : Application
{
    private void ShowFatal(Exception ex)
    {
        try
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JASS", "BookmarkMaster");
            Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "startup-error.log");
            File.AppendAllText(logPath, $"{DateTime.Now:O}\r\n{ex}\r\n");
            MessageBox.Show($"An application error occurred.\n\n{ex.Message}\n\nLog: {logPath}", "JASS Bookmark Master", MessageBoxButton.OK, MessageBoxImage.Error);
        } catch { }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) => { args.Handled = true; ShowFatal(args.Exception); };
        try
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JASS", "BookmarkMaster");
            Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "startup-error.log");
            File.WriteAllText(logPath, $"{DateTime.Now:O}\r\n{ex}\r\n");
            MessageBox.Show($"JASS Bookmark Master could not start.\n\nDetails were saved to:\n{logPath}\n\n{ex.Message}", "JASS Bookmark Master", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
