# EDL-to-YT
Youtube has recently added an auto-chapter feature that requires timestamps in the description (then they do some parsing magic).

Exporting the markers from a NLE (ie Davinci Resolve) to use those markers as the timestamps is technically human readable, but a mess, and takes far too long to mentally parse.
This utility simply takes the EDL text, converts it into what youtube wants. Additional filtering for colours (ie, make blue chapter titles, yellow other notes)

Online version found at https://thewoodknight.github.io/EDLtoYT/

This is a client-side Blazor app, because... it is.
