# Menu preview inspection

Captured from the actual Unity 6000.3.27f1 Windows preview on 2026-10-23.
These are rendered game views, not concept images. All example versions and release
notes are explicitly simulated; no update was installed or release published.

| Capture | Inspection |
|---|---|
| [Menu](menu.png) | Painted backdrop, readable panel and honest build exploration entry at 1280 x 720 |
| [Settings](settings.png) | Shared settings navigation and unavailable language option |
| [Build exploration](play.png) | Explicitly states that gameplay is not available |
| [Update dialog](popup.png) | Download/restart consent, initial Later focus and modal scrim |
| [Checking](checking.png) | Busy state disables duplicate requests |
| [Available](available.png) | Explicit download action and simulated release history |
| [Downloading](downloading.png) | Text percentage and restrained progress bar at 47% |
| [Ready](ready.png) | Restart-to-install state; production retains automatic apply after consent |
| [Current](current.png) | No available update; release history remains accessible |
| [Failure](error.png) | Retry and installed-version reassurance |
| [Warning](warning.png) | Original settings preservation warning before page content |
| [Unsupported](development.png) | Update action disabled without assuming a desktop executable |
| [Portrait](narrow.png) | 390 x 844, wrapped title and readable controls |
| [Small window with cutout](cutout.png) | 640 x 480, simulated 50-pixel side and 25-pixel vertical insets; scrollable content |
| [Portrait dialog](popup-narrow.png) | Consent and Later remain accessible at 390 x 844 |
| [Keyboard focus](keyboard-focus.png) | Native interactive capture: arrow-key focus and automatic scroll reveal |
| [Initial menu](menu-wide.png) | Native interactive capture before a simulated update was offered |

Interactive inspection also verified keyboard activation with Enter and ordinary
mouse navigation. The final matrix was produced by Preview tools / Capture view
matrix; see [Development](../../development.md#painted-menu-inspection) to reproduce.
The two native captures include window chrome; matrix images are game framebuffer
captures. Preview-only tools, simulation data and the development watermark are not
part of production builds. The source background and generation prompt are recorded
in [ASSETS.md](../../../ASSETS.md#painted-menu-backdrop).

No mobile build, store integration, gameplay, release installation or N-to-N+1
update support is established by these visual inspections. Automated behavioral
checks and both production platform builds run only in final Prepare merge validation.
