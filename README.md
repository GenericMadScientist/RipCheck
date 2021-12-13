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
- Check for unknown notes

More will be added over time.

## Install

Grab the executable from the [Releases page](../../releases).

## Usage

Specify the directory containing the mids and RipCheck will recursively go
through every .mid file contained within. An example usage is

```bat
> RipCheck.exe "D:/RB conversions/"
```

If you want to only check .mid files specifically named notes.mid, pass the
option `--notesonly`.

All warnings will be printed to the console. It is advised to redirect the
output to a file.
