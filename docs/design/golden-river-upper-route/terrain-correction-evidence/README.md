# Terrain correction review evidence (owner rejected)

This candidate was rejected on 2026-11-14. It is retained only as historical evidence. See [the current rebuild](../cave-rebuild.md); these captures and test results do not validate the new geometry.

This replaces the owner-rejected `permanent-shore-evidence` candidate. See [scope, implementation and remaining review](../terrain-corrections.md) and [new stone provenance/prompt](../terrain-stone-assets.json).

## Captures

Sixteen actual Unity camera poses have 960x540 color, 640x360 color (`-640.png`) and 640x360 grayscale (`-gray.png`) variants. Deliberately placed composition poses are separate from traversal proof.

| Pose | Subject |
| --- | --- |
| 0–1 | Entrance bank and descending shore |
| 2 | Fountain and chamber |
| 3–4 | Central takeoff and hidden catch after a miss |
| 5–7 | Exit approach, sharp outer steps and rejoin |
| 8 | Upstream main bank and river |
| 9 | Connected recovery staircase and attached overhangs |
| 10–11 | Permanent outer mouth in both directions |
| 12 | Main route above the chamber, with the intrusive backdrop removed |
| 13 | Former vertical floor-image cut before the fountain |
| 14 | Former mismatched return-bank seam |
| 15 | Main route above the exit, with the old clipped catch shelf removed |

The four clips run at normal speed, sampled at 10 fps from 0.02-second actual physics steps. `forward-route.mp4` covers the upstream approach through the distant rejoin; `exit-return.mp4` returns through the same mouth; `recovery.mp4` tests the central miss and complete recovery staircase; `entry-return.mp4` climbs back through the entrance. Exact frame-count files from the source sequences exclude stale trailing frames. Water and dust use the simulation clock. This is not an FPS benchmark or human playtest.

The direct scripted forward route remains approximately 48.64 seconds including the approach. Human duration and the earlier roughly 60-second pacing target remain separate review points. See `physics.txt` for the executed physics-review statement.

## Executed validation

- Pinned Unity 6000.3.27f2 full local suite: **236/236 passed**, zero failures or skipped tests. This includes the complete main route with and without the cave, the actual recovery/reverse paths, and the backdrop-bound regression assertion.
- The recovery test initially caught insufficient headroom under the new attached return overhang. Its underside was corrected, and the final full route/recovery recording and complete suite passed afterward.
- Windows development build and packed-art audit passed: 81 active paintings, archived art excluded. Imported descent textures total 111,182,208 bytes. The 16,777,216-byte RGBA mask and six 65,536-byte contact profiles bring the conservative combined texture allocation to approximately **122.41 MiB**, below 128 MiB. This is not an FPS or complete process-memory measurement.
- The old generic catch-shelf landing test excludes exit group 7 because the actual native exit/re-entry tests now cover its replacement terrain; other tested lift/recovery groups remain. Panorama checks retain their original two-panorama requirement.
- The rebuilt Windows player was opened from `artifacts/shore-correction-preview/LegendOfCatPreview.exe` with `--preview-view level-one-descent`; its actual visible game window was inspected. It remains open for owner testing. This is not a completed human playthrough or visual approval.

No owner visual acceptance or release readiness is claimed. Entry/shore composition remains under review. PR #79 stays draft; exact-head Windows/Linux player and launcher builds, package/merge-ready checks and same-hardware performance comparison remain later final gates. No merge or release is authorized.
