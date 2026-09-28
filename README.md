# JASS Bookmark Master v1.0

A beautiful, local-first WPF desktop application for building one master bookmark library from multiple browser/resource bookmark collections.

## v1.0 features
- SQLite master library stored under `%LOCALAPPDATA%\\JASS\\BookmarkMaster`
- Add bookmarks manually
- Import HTML bookmark exports from Chrome, Edge, Firefox and compatible browsers
- Import/export JSON master files
- Export a portable HTML bookmark file
- Search title, URL, folder, tags and notes
- Favorites and Read Later flags
- Folders and tags
- Broken-link flag
- Duplicate-safe URL insertion
- Dashboard statistics
- Open bookmarks in the default browser
- Local/private by default

## Design direction
The database is the authoritative master library; JSON is the portable backup/interchange format. Browser bookmark files are treated as import sources rather than separate permanent stores.

## Build
```powershell
dotnet restore
dotnet build
dotnet run
```

## v1.1 — Local Resource Library

JASS Bookmark Master can now search your local folders recursively and turn files or folders into bookmarkable `file:///` links.

### Local Resources
- Choose any accessible Windows folder.
- Recursive search through subfolders.
- Search by file/folder name or path.
- Optional folder results.
- Displays type, extension, size, modified date and containing folder.
- Double-click a result to open it with Windows.
- Add selected resources to the master bookmark database.
- Add all visible search results in one operation.
- Local bookmarks are stored in the same SQLite master library as web bookmarks.
- Existing local links are skipped automatically.

This keeps one master library for web URLs and useful local resources such as documents, PDFs, project folders, images, installers, scripts and reference files.
