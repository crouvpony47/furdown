# furdown
Yet another mass downloader for FurAffinity.net.

**Note**: only works with the new (Modern) theme.

[Download stable win32 builds.](https://github.com/crouvpony47/furdown/releases)

### Changelog (v.2.0.1.0)
- Switched to Edge-based embedded browser for authentification and human verification.

### A note about CF's "I'm Under Attack" mode
- If you are already logged in but are shown the login form anyway, simply navigate to the FA front page.

### System requirements
- Windows 7 SP1, 8.1, or 10+. Windows 10 or 11 recommended.
- .NET Framework 4.6.2
- Microsoft Edge WebView2 (usually preinstalled on Windows 10 or newer)

Edge WebView2 requirement can be bypassed if you implement an alternative cookie provider, see "Advanced options" section below.

### Portable mode

To leave no traces in ApplicationData directory you can create an empty file called `furdown-portable.conf` in the working directory, it will then be used as a settings storage file instead of the default `%AppData%\furdown\furdown.conf`.

### Batch/CLI mode
v.0.3.6 adds a basic support for being run within a batch script, or from a terminal.

For that `-b` must be passed as the first argument, followed by a combination of one or more arguments:

- `-g URL`

Download a gallery / scraps / folder located at URL. If URL is prefixed with $, it will be treated like a file to read the url list from.

- `-d`

Tells furdown you also want submission descriptions saved as HTML.

- `-u`, `-upd`, `-update` (since 0.4.9.9b)

Check for content updates. This means comparing the file IDs with the ones stored in furdown's internal cache, and downloading the changed files. [(details)](https://github.com/crouvpony47/furdown/issues/15)

- `-f FILE`

Download all galleries listed in FILE. Same as `-g $FILE`

- `-o DIR`

Override downloads directory with DIR.

- `-dbignore`, `-dbi`

Prohibits re-downloading files already marked as downloaded in furdown's internal database.

- `-dbforce`, `-dbf`

Allows re-downloading files already marked as downloaded in furdown's internal database.

- `-st TMPLT`

Override submissions naming template with TMPLT. __*__ 

- `-dt TMPLT`

Override descriptions naming template with TMPLT. __*__

__*__ As templates use the syntax much like that used for environment variables, you may get unexpected results if, say, %FILEPART% is an existing variable.

**Other notes**:
- You must login in GUI mode at least once before using batch/CLI mode unless the cookies/user-agent are provided using the environment variables.
- For non-overridden parameters the value from a current configuration file (the same file used in GUI mode) is used.

**Examples**:
- `furdown.exe -b -g https://www.furaffinity.net/gallery/flipstick`
- `furdown.exe -b -d -dbi -f "Z:\Saved Galleries\list.txt" -o "Z:\Saved Galleries\"`
- `furdown.exe -b -d -g https://www.furaffinity.net/scraps/flipstick -dt %ARTIST%.s\%SUBMID%.%FILEPART%.dsc.htm -st %ARTIST%.s\%SUBMID%.%FILEPART%`

Note that `%` might need to be escaped as `%%` in batch scripts.

### Advanced options

The builtin authentication mechanism based on the embedded Edge can be bypassed by setting `FURDOWN_COOKIES` and `FURDOWN_USERAGENT` environment variables to the appropriate values. Setting just the `FURDOWN_USERAGENT` has no effect.

Note that the `FURDOWN_COOKIES` is expected to contain the entire cookie header value (something like `b=XXX; __gads=XXX; a=XXX; s=XXX; __qca=XXX; sz=XXX; cc=XXX; __cfduid=XXX` where `XXX`s are some values)
