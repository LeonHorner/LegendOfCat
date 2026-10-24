# Mika presentation integration review

Follow-up to merged PR #41 and the user's 2026-10-24 review. Related issue #10
remains open. The approved four-frame run and apex pose remain unchanged.

## Scope and coordination

This branch owns `MikaSpritePresentation.cs`, a dedicated integration test file,
an Editor-only movement recorder and review evidence. PR #44 owns controller,
retry lifecycle, input and movement-course changes. Do not independently modify
those components or its existing tests. Integration tests consume their public
behavior and the actual Resources/MovementCourse and Resources/MikaOriginal
prefabs. Visual inspection additionally exposed a stray-pixel pivot in original
run pose 6. This branch owns the Editor contact-pivot helper and its regenerated
sprite metadata; #44 does not modify them. No settings, scene, package or new
artwork changes are planned.

## Review tracking

- P1: test initialization, actual sprite references, running, blocking contacts,
  jump/apex/descent/landing and retry together with real 2D physics.
- P2: measure presentation correction on flat surfaces, edges and sloped
  contacts, including grounded/airborne transitions. Fix discontinuous or
  unsupported floor corrections without changing physics or deforming artwork.
- P2: move rejected rig/optical-flow experiments out of Unity's imported asset
  paths in a separate cleanup PR, preserving sources, metadata and provenance.
  This branch does not add to the experimental asset collection.
- P2: provide a recording of the current integrated character. Full visual
  acceptance and final authored jump/landing sequences remain part of #10.
  Full Windows/Linux player/launcher validation must run for the final ready
  revision. Focused tests and static checks are not a replacement for it.

## Validation

Unity 6000.3.27f1: 18 focused Mika tests passed (four real-prefab integration
tests, nine animation-state tests and five retained historical rig tests).
The historical tests do not establish artwork acceptance. Integration cases cover
the actual renderer references, opaque support-paw landmarks in all four running
poses, movement, blocking contact, reversal, jump/apex/descent/landing, retry,
scaled geometry, platform-edge departure and a 20-degree slope.

The course test initially checked only the sprite pivot. Recording inspection
exposed a false positive: original run pose 6 had a pivot at pixel y=0.5 while
its connected supporting paw was at y=39.5, about 0.194 m above the floor at the
imported scale. The importer now derives the sole from the largest connected
opaque silhouette. Only original-frame-06.asset changes; the original drawing,
four-pose sequence, stride, airborne pivots and controller remain unchanged.
Independent known opaque paw-pixel fixtures now verify actual visible contact,
so aligning an empty pivot can no longer satisfy the flat-ground run test.

Runtime floor correction is limited to 0.04 m around the transformed collider
sole, ignores remote slope corners and returns toward the collider anchor at
0.4 m/s. The unexplained fixed downward offset is removed. This is contact-skin
correction, not a system for making every foot of a rigid image conform to slopes.
The tests require bounded correction and continuous support changes on slopes;
they do not claim all four depicted paws meet arbitrary sloping geometry.

## Movement evidence and reproduction

- [Integrated playback](integrated-movement-review.mp4): six seconds, 360 frames
  at 60 Hz, using real Resources course and character prefabs in local 2D physics.
- [Review strip](integrated-review-strip.png) and
  [grayscale strip](integrated-review-grayscale.png): crops at original render
  scale, without anatomical edits. The character is deliberately small.
- [Recording metadata](integrated-review.json): phase/source summary and limits.

Run `LegendOfCat/Mika/Record integrated movement review` or invoke
`MikaMovementReview.Record` with Unity's `-executeMethod` in batch mode, with
graphics enabled. It writes PNG frames and playback.csv to ignored
Logs/mika-integrated-review. Encode with:

```powershell
ffmpeg -framerate 60 -i Logs/mika-integrated-review/frame-%04d.png -c:v libx264 -crf 20 -pix_fmt yuv420p docs/design/mika-animation/integrated-movement-review.mp4
```

Tracking is scripted for inspection; Camera.Render omits the IMGUI HUD. This is
deterministic physics playback, not a human playtest or a capture of menu flow.
Review confirms the four source run poses, distinct ascent/apex/descent, contact
and recovery are present. Jump/landing drawings still need artistic acceptance;
some source specks/halos remain. The contact fix does not repaint that artwork.

Full exact-head CI and Windows/Linux player/launcher builds have not run for this
draft. Rejected experiment cleanup and final issue #10 acceptance remain open.
