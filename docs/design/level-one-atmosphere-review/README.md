# Golden River completion candidate

These are actual Windows development-player screenshots of the first #74 candidate. The preview starts three course units before the real exit and walks through it; it does not simulate a complete player run or measure human timing. The sub-second timer belongs to this focused preview.

- [960×540 completion](complete-960.png)
- [640×360 completion](complete-640.png)

Reproduce with the pinned Unity BuildGame.BuildPreview method, then launch the development executable with:

```text
-force-d3d11 -screen-fullscreen 0 --preview-view level-one-complete --preview-width 960 --preview-height 540 --preview-capture <absolute-output.png>
```

Use 640 and 360 for the smaller capture. On this machine hidden windows and the initial D3D12 capture produced black/stale frames; those were discarded. The inspected screenshots use a visible D3D11 player. No terrain/art approval is inferred from this UI review. In-game sound/mix acceptance and the complete final Windows/Linux validation remain open on draft PR #89.
