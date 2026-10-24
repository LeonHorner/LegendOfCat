# Mika presentation integration review

Follow-up to merged PR #41 and the user's 2026-10-24 review. Related issue #10
remains open. The approved four-frame run and apex pose remain unchanged.

## Scope and coordination

This branch owns `MikaSpritePresentation.cs`, a dedicated integration test file,
an Editor-only movement recorder and review evidence. PR #44 owns controller,
retry lifecycle, input and movement-course changes. Do not independently modify
those components or its existing tests. Integration tests consume their public
behavior and the actual Resources/MovementCourse and Resources/MikaOriginal
prefabs. No settings, scene, package or new artwork changes are planned.

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

Implementation and evidence pending. Do not infer passing results from this plan.
