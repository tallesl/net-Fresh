# FolderCleaner

[![build](https://ci.appveyor.com/api/projects/status/github/tallesl/FolderCleaner)](https://ci.appveyor.com/project/TallesL/FolderCleaner)
[![nuget package](https://badge.fury.io/nu/FolderCleaner.png)](http://badge.fury.io/nu/FolderCleaner)

A .NET library that periodically cleans up a folder.
Useful for cleaning up temporary files.

## Usage

```cs
using FolderCleaning;

var cleaner = new FolderCleaner("Temp/", TimeSpan.FromMinutes(30), TimeSpan.FromHours(2), FileTimestamps.Creation);
```

This creates and starts a cleaner that, every `2` hours, deletes all files in `Temp/` that has been `created` more than `30` minutes ago.

Note that the cleaner already starts cleaning after it's construction.

Remember to call `Dispose()` on it.
