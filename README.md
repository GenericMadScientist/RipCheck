# RipCheck

A program to automatically check for various issues in rips for Clone Hero.

## About

If you're not converting songs from games like Guitar Hero and Rock Band for use
in Clone Hero, this is probably not of any interest to you. This project is
specifically to aid in that effort.

Current checks include:

- Make sure the .mid resolution is 480
- Check for chord snapping problems
- Check for disjoint chords
- Check for unknown notes (disabled by default)
- Check for overlapping notes of the same color
- On vocals, check for misaligned lyrics/notes and notes outside of phrases

More may be added over time.

## Install

Grab the executable from the [Releases page](../../releases).

## Usage

Specify a directory and RipCheck will recursively go
through every .mid file contained within:

```bat
RipCheck.exe "D:/RB conversions/"
```

You can specify parameters before the path to enable/disable specific checks, like so:

```bat
RipCheck.exe -n -a "D:/RB conversions/"
```

All warnings will be printed to the console. It is advised to redirect the
output to a file:

```bat
RipCheck.exe -n -a "D:/RB conversions/" > warnings.txt
```

The options are as follows:

| Parameter         | Description                                                                     |
| :--------         | :----------                                                                     |
| `n`/`--notesonly` | Only scan files called "notes.mid".                                             |
| `a`/`--checkall`  | Enables checking of Pro instruments and unknown notes. Overrides `-g` and `-r`. |
| `r`/`--rbpro`     | Enables checking of Pro instruments.                                            |
| `g`/`--ghband`    | Excludes the disjoint chord check for GHWT and onward songs.                    |
