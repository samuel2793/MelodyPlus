# MelodyPlus
Cross platform floating [Spotify](https://spotify.com) now playing widget

# Platforms
- Windows x64
- Linux x64

# Releases
## [1.0.0-beta.2](https://github.com/Thelonedevil/MelodyPlus/releases/tag/1.0.0-beta.2)
The second beta release of Melody Plus with support for Window x64 and Linux x64.

### Changes
- New Icon
- Now stores settings in the MelodyPlus folder in the system specific local application directory 

Windows:
```
C:\Users\<user>\AppData\Local\MelodyPlus\
```
Linux:
```
~/.local/share/MelodyPlus/
```
### New Features
- Rudimentary language selection
### Known Issues
- Language selection does not remember your selection between application launches
- Auto sizing does not work with X11 [see framework issue](https://github.com/AvaloniaUI/Avalonia/issues/1748) 

## [1.0.0-beta.1](https://github.com/Thelonedevil/MelodyPlus/releases/tag/1.0.0-beta.1)
### New Features
- Customisable layout
- Dark/Light Mode
- Auto colour theming based on current album art
- Playlist share QR Code

### Known Issues
- Auto sizing does not work with X11 [see framework issue](https://github.com/AvaloniaUI/Avalonia/issues/1748) 
