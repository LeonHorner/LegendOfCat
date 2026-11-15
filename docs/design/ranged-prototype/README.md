# Ranged lab player review - 2026-11-15

Actual Windows development-player captures from Unity 6000.3.27f2, Direct3D 11.
Both 960x540 and 640x360 are actual player window sizes, not resized screenshots.
The compact clips contain 100 frames sampled at approximately 0.1-second intervals.
The grayscale version converts the same rendered frames without changing contrast.

- [Color motion](combat-640.gif)
- [Grayscale motion](combat-640-grayscale.gif)
- [Shielded target](shielded-640.png)
- [Partial charge](charge-640.png), [grayscale](charge-640-grayscale.png)
- [Reload progress](reload-640.png)
- [High grounded hit at 640x360](hit-640.png), [960x540](hit-960.png)

Updated after the approved weapon-switch/reload decision. J uses the active
weapon, K switches and R reloads early. Pulse: 8 rounds, 0.2-second recovery,
1-second reload. Orb: 3 rounds, 0.4-second recovery, 1.5-second reload.

The scripted demonstration forwards the shared attack actions and movement/jump
signals to the actual runtime components. It shows a grounded charged orb hitting
an elevated target, target health changing from 6 to 5, movement, jumps, facing
changes, direct shots, early reload, switching during reload and automatic
reload after the pulse magazine is emptied. It is not a human usability test.
The recorder is compiled only into preview builds. No target damage is injected.

The charge readout and target state remain visible at the compact size. Round
orbs and narrow pulses remain distinct in grayscale; closed protection also has
a square silhouette and SHIELDED label. Landing edges and Mika remain unobscured.
Both actual-player logs contain no exceptions or error entries. The preview's
packed-art audit passed. No Linux graphical playtest or install/update smoke test
is claimed by these captures.

Local focused tests passed 12/12, including per-weapon magazines/recovery,
automatic/early reload, abandoned reload progress, switching while charging,
real J/K/R and controller callbacks, and menu lifecycle. Other modes keep R retry.
The exact-head full-suite and both platform player/launcher/package results
belong to PR #88's updated Prepare merge run; the earlier green head is superseded.

Reproduce after BuildGame.BuildPreview:

```text
LegendOfCatPreview.exe -force-d3d11 --preview-view ranged --ranged-record <absolute-frame-directory>
LegendOfCatPreview.exe -force-d3d11 --preview-view ranged --ranged-compact --ranged-record <absolute-frame-directory>
```

These commands run the scripted demonstration and exit. To play manually, launch
the preview normally and choose Ranged combat lab, or use --preview-view ranged
without recording arguments. Player feel and human simultaneous movement, jump
and attack usability still need owner playtesting; this prototype does not select
mobile touch controls or approve production bosses.
