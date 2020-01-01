# furdown
Yet another mass downloader for FurAffinity.net.

**Note**: only works with new/beta theme.

[Download stable win32 builds.](https://github.com/crouvpony47/furdown/releases)

### Changelog (v.0.4.0.0)
- adapt to the FA template changes

### System requirements
- Windows Vista SP2 or newer, might not work on server editions
- .NET 4.5
- *Having IE11 installed is strongly recommended for better login experience*

### Portable mode

To leave no traces in ApplicationData directory you can create an empty file called `furdown-portable.conf` in the working directory, it will then be used as a settings storage file instead of the default `%AppData%\furdown\furdown.conf`.

### Batch/CLI mode
v.0.3.6 has a basic support for being run within a batch script, or from a terminal.

For that `-b` must be passed as the first argument, followed by a combination of one or more arguments:

- `-g URL`

Download a gallery / scraps / folder located at URL. If URL is prefixed with $, it will be treated like a file to read the url list from.

- `-d`

Tells furdown you also want submission descriptions saved as HTML.

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
- You must login in GUI mode at least once before using batch/CLI mode.
- For non-overridden parameters the value from a current configuration file (the same file used in GUI mode) is used.

**Examples**:
- `furdown.exe -b -g https://www.furaffinity.net/gallery/flipstick`
- `furdown.exe -b -d -dbi -f "Z:\Saved Galleries\list.txt" -o "Z:\Saved Galleries\"`
- `furdown.exe -b -d -g https://www.furaffinity.net/scraps/flipstick -dt %ARTIST%.s\%SUBMID%.%FILEPART%.dsc.htm -st %ARTIST%.s\%SUBMID%.%FILEPART%`
