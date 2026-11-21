# Connected cave evidence

Current permanent level connection, 2026-11-13. This is a scripted review candidate; only the earlier entrance has human visual acceptance.

`view-0` is the main-path entrance, `view-1` the initial descent, `view-2` the fountain room, `view-3` the central takeoff, `view-4` its concealed catch after falling, `view-5` the ascending return, `view-6` the last approach and `view-7` the later main bank. Default stills are 960x540, `-640` stills 640x360 and `-gray` stills 640x360 grayscale.

`connected-route.mp4` is the complete entrance-to-rejoin route at actual physics speed (10 fps presentation, 50 Hz physics, approximately 45.1 seconds). It is not human input, an FPS measurement or a recording of the entire level.

Reproduce capture and real-collision checks with `IntegratedCaveReview.Record` in the pinned Editor. Run `IntegratedCaveReview.Verify` for the non-capture integrated movement/recovery checks. Test the actual Windows player with `--preview-view level-one-descent`; the player shares the real main checkpoints, retry, restart and finale.
