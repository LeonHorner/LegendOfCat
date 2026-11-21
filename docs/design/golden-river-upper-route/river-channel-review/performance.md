# Linux cave rendering

The connected cave used to draw the surface panoramas behind its opaque rear wall, then shade the wall on top. The interior now writes depth before the panoramas draw. The last few metres still blend with the valley. The rock shader also has separate programs for shelves, banks, rock edges and the chamber wall. Only the waterfall chamber samples the waterfall painting.

## Measured camera draw time

The baseline was PR revision `86774db`. Both Linux development players ran on the same integrated AMD Radeon using OpenGL Core, Mesa 26.2.7 and Unity 6000.3.27f2. Each 1920 × 1080 camera position has 270 timed draws per build, pooled across two launches. The launch order was baseline, optimized, optimized, baseline. Lower times are faster.

| View | Median before | Median after | 95th percentile before | 95th percentile after |
| --- | ---: | ---: | ---: | ---: |
| Surface reference | 4.08 ms | 3.70 ms | 6.33 ms | 6.21 ms |
| Entrance | 9.64 ms | 6.47 ms | 12.28 ms | 9.41 ms |
| Fountain | 11.03 ms | 6.32 ms | 18.25 ms | 9.02 ms |
| Waterfall | 8.69 ms | 5.38 ms | 11.64 ms | 9.34 ms |
| Spillway | 11.50 ms | 7.13 ms | 14.94 ms | 8.37 ms |
| Exit | 9.64 ms | 8.56 ms | 12.74 ms | 12.14 ms |

A second comparison at 1280 × 720, with 135 timed draws per view, measured median waterfall draw time of 6.89 → 3.01 ms and fountain draw time of 7.37 → 4.08 ms. The surface reference varied from 2.23 to 3.01 ms at that resolution. The timer covers the game camera draw and a synchronous one-pixel GPU readback. Display synchronization, input latency and whole-game frame pacing require a separate measurement.

Matched 1080p screenshots retained the grass edge, painted roof, waterfall, spillway and translucent exit. Across the five cave views, the largest mean absolute RGB difference was below 0.03% of the full channel range.

## Packaged assets

The previous build still shipped five unused entrance paintings and four masks. They are now in the art archive. The clean build audit checks both that selected artwork is present and that every packed cave PNG belongs to the selection. The Linux resource data file shrank from 501,772,224 to 469,271,776 bytes, a reduction of 31.0 MiB. Other study paintings are used by the separately selectable entrance review. The bound-material audit found 19 cave-folder textures on the connected level, including its three masks. The separate study’s paintings were absent from those materials.

## Repeat the comparison

In an isolated project copy, copy the [timing probe](../../../../tools/CaveRenderTiming.cs) into `Assets/Game/Scripts/` before building the Linux cave preview. Use the same probe on each revision. The regular playtest build omits it.

```sh
mkdir -p "$PWD/artifacts/cave-timing"
./artifacts/cave-perf-linux/GoldenRiverCave.x86_64 -batchmode -force-glcore \
  --cave-render-timing "$PWD/artifacts/cave-timing" 1080 \
  -logFile "$PWD/artifacts/cave-timing/player.log"
```

Use 720 as the final timing argument for the smaller target. Each run writes six screenshots and a CSV of per-draw timings. The probe visits the views three times, reverses their order on the middle pass, and discards the first twelve draws at every stop.
