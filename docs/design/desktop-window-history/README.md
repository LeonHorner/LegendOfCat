# Desktop window and release history inspection

Actual Windows Unity 6000.3.27f1 preview-player captures from 2026-10-23.
The preview uses 60 synthetic release cards, including PR #35 download headings,
conventional changelog headings, lists, emphasis, links and commit references.

- [1280 x 720](history.png)
- [1280 x 720 scrolled to the bottom](history-bottom.png)
- [960 x 540 target minimum](history-minimum.png)
- [2560 x 1080 ultrawide](history-wide.png)
- [540 x 720 narrow](history-narrow.png)

Back to settings stays at the top when the history reaches its last card. Only
the content area scrolls. Cards wrap within the panel. Download instructions,
Markdown delimiters, raw URLs and commit hashes are absent. Links open full notes
on GitHub. No release/update was published by this local preview.

These captures verify layout, not real Linux compositor behavior, operating-system
shortcut activation, installed shortcut appearance or N-to-N+1 update support.
Manual Windows/Linux real-install and update checks remain in RELEASE-TEST.md.
The documented responsive fallback deliberately does not enforce native minimum
window dimensions or repeatedly resize the OS window.
