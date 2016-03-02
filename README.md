# Fresh

[![][build-img]][build]
[![][nuget-img]][nuget]

A .NET library that periodically cleans up a folder.
Useful for cleaning up temporary files.

[build]:     https://ci.appveyor.com/project/TallesL/net-fresh
[build-img]: https://ci.appveyor.com/api/projects/status/github/tallesl/net-fresh?svg=true
[nuget]:     https://www.nuget.org/packages/Fresh
[nuget-img]: https://badge.fury.io/nu/Fresh.svg

## Usage

```cs
using FreshLibrary;

var cleaner = new FreshFolder("Temp", TimeSpan.FromMinutes(30), TimeSpan.FromHours(2), FileTimestamps.Creation);
```

This creates and starts a cleaner that, every `2` hours, deletes all files in `Temp` folder that have been `created`
over `30` minutes ago.
The cleaner already starts its cleaning after its construction.

Remember to `Dispose()` it when you're done.
Consider disposing the cleaner on `Application_End` or [`ProcessExit`].

[`ProcessExit`]: https://msdn.microsoft.com/library/System.AppDomain.ProcessExit