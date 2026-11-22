# Ranged lab player review

The owner accepted this initial prototype on 2026-11-15. Finalization resumed on
2026-11-21. Controls are J/left click for the paw pulse and K/right click
hold/release for the orb, with no explicit weapon switching or F/Q attacks.
R reloads the most recently used attack; using the other interrupts charge/reload.

Actual Windows development-player captures use Unity 6000.3.27f2 and Direct3D 11.
The accepted runtime's 960x540 captures and enlarged pause-menu captures were made
on 2026-11-15; current 640x360 motion was captured on 2026-11-21. Both sizes are
actual player windows, not resized screenshots. The compact clips contain 100
frames sampled at approximately 0.1-second intervals. Grayscale converts the same
frames without changing contrast.

- [Color motion](combat-640.gif), [grayscale motion](combat-640-grayscale.gif)
- [Shielded target](shielded-640.png)
- [Partial charge](charge-640.png), [grayscale](charge-640-grayscale.png)
- [Reload bar and empty magazine](reload-640.png)
- [Elevated grounded throw at 640x360](hit-640.png), [960x540](hit-960.png)
- [Enlarged pause menu and input descriptions](pause-controls-640.png)

The preview-only recorder forwards the shared attack and movement/jump actions
to runtime components. It demonstrates an elevated grounded orb, movement, jumps,
facing, direct pulse shots, charge/release and automatic/early reload. No damage
is injected. Scripted clips are evidence of rendering and reachability, not a
human usability test.

The bottom-center HUD shows both magazines using paw/orb symbols, filled/hollow
dots, charge/reload percentages and a reload progress bar. There is no permanent
blue bar in front of Mika. The enlarged pause menu puts actions beside keyboard,
mouse and controller guidance at wide sizes, with scrolling on smaller surfaces.
Round orbs and narrow pulses stay distinct in grayscale; protection uses a square
silhouette and SHIELDED label. Landing edges and Mika remain unobscured in play.

The accepted preview build and packed-art audit passed. The final compact player
log has no runtime exceptions/error entries. Focused direct-attack regression
tests passed 12/12 on 2026-11-21, including real mouse/keyboard/controller actions,
move/jump/charge, safe paused clicks, independent recovery, interrupted reload and
menu lifecycle. Earlier passing CI for weapon-switch controls is superseded.
The final exact-head full-suite and Windows/Linux player/launcher/package/Delta
results belong to PR #88's Prepare merge run.

No Linux graphical playtest or install/update smoke result is claimed here.
This prototype does not select mobile touch controls or approve production bosses.

Reproduce after BuildGame.BuildPreview:

```text
LegendOfCatPreview.exe -force-d3d11 --preview-view ranged --ranged-record <absolute-frame-directory>
LegendOfCatPreview.exe -force-d3d11 --preview-view ranged --ranged-compact --ranged-record <absolute-frame-directory>
LegendOfCatPreview.exe -force-d3d11 --preview-view ranged-paused --ranged-compact --preview-capture <absolute-path>.png
```

Recording arguments run the scripted demonstration and exit. For manual play,
choose Ranged combat lab in the main menu, or use --preview-view ranged without
recording arguments. Press P to see the enlarged pause menu.
