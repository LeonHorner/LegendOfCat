# Mika character authoring

Issue #10 is in progress. This is isolated authoring work, **not accepted production artwork**.

## Ownership and integration

`feat/mika-animation` owns `MikaAnimation.cs`, `MikaSpritePresentation.cs`,
`MikaCharacterPreview.cs`, `MikaAnimationTests.cs`, `Assets/Game/Art/Mika/` and this
directory. PR #36 retains `MovementCourse.cs`, camera/display integration and its
`ASSETS.md` changes. PR #37 retains launcher/release/update workflows. Coordination
notes were posted on both PRs before edits. No shared scene, prefab, package,
project setting, movement rule, controller or input implementation is modified.

After #36 is safely integrated, the character branch owner should merge current
main normally, preserve its framing/HUD changes, replace the temporary geometry
with a cosmetic child, and append the staged provenance to `ASSETS.md`. Never
replace that file wholesale. Do not merge either PR as part of this task.

## Preview and presentation

Use Unity 6000.3.27f1 and **LegendOfCat > Mika > Create isolated character preview**.
The command imports the candidate uncompressed with alpha, creates sixteen
registered sprite assets and a dedicated candidate prefab, then opens an unsaved
preview scene after offering to save existing work. It does not modify the movement
course. Enter Play mode and use A/D, jump and retry controls from `CatInput`.

`MikaAnimation` selects idle, run, turn, jump, fall and landing. The adapter observes
the existing controller, measured horizontal displacement and grounded state.
Facing changes immediately. Ground flourishes cannot delay a jump. Gait phase
uses distance (provisional 2.4 m stride), avoiding clock-driven running against a
wall. Retry resets presentation history. Sprite flipping never scales the body,
collider or whole character. No animation event controls gameplay or hit timing.
The component must live on a visual child, never the physics root.

These rules do not establish that the candidate's painted feet are stationary:
source registration and authored contact phases still require correction and
motion review. Four run frames are an authoring candidate, not a claim of smooth
final animation. Airborne frames and turn silhouettes also need proportion review.

## Remaining acceptance work

- Correct clipped run tails, stray pixels between run cells, cell margins and
  cross-frame registration. These defects are visible in the Unity contact sheet.
- Author and review enough contact/passing poses for smooth running with planted
  feet at slow, full and reversing speeds; tune stride to the actual art.
- Review idle/turn/jump/fall/landing transitions without body stretching or scale drift.
- Integrate safely with #36 and append staged provenance to `ASSETS.md`.
- Record actual movement, short/buffered jumps, reversal, fall, landing and retry.
  Inspect normal, small-window and grayscale views against light and dark backdrops.
- Run exact-head final Prepare merge checks and both platform builds when complete.

No extra idle/fish behavior, new ability, combat timing or campaign decision is introduced.

## Staged asset provenance

Asset: `Assets/Game/Art/Mika/mika-movement-candidate.png` and Editor-derived frames.
Source: built-in OpenAI image generation, 2026-10-23 (Europe/Vienna); exact model ID
not exposed. License/terms: `ASSET-LICENSE.md` where rights exist; generation does
not establish exclusivity. No third-party asset, character, font or music imported.
Editor slicing is lossless and retains source alpha. Human direction selected
natural orange-and-cream quadruped proportions, modest head/eyes and striped tail
from archived references. Implementation choices: side-on right-facing poses,
seven movement states, cosmetic separation, no extra idle/fish behaviors. The first
generated variant was rejected for crowded cells; the retained second variant
still needs cleanup and is explicitly a candidate. The source generation prompt
is archived in [generation-prompt.txt](generation-prompt.txt).

`ASSETS.md` is intentionally deferred under the shared-file ownership agreement;
copy this entry there at integration. Do not describe the issue as closed before
the remaining criteria and provenance integration are complete.

## Local validation and capture

Unity 6000.3.27f1 compiled the runtime, Editor and test assemblies and completed
candidate import/prefab generation successfully. `Packages/packages-lock.json`
was unchanged after import. The source/documentation whitespace check passed;
the complete diff reports Unity-generated empty-field trailing spaces in metadata,
sprites and the prefab, which are preserved as generated. Tests were added but not
executed; no player/launcher builds or full final validation were run for this
unfinished draft.

![Unity-rendered candidate contact sheet](candidate-preview.png)

This is an actual Unity Camera.Render capture, created by **LegendOfCat > Mika >
Capture candidate contact sheet**, not a movement recording. Rows show idle, run,
turn/jump/fall and landing/recovery candidates. The source sheet's uneven margins
cause cropped tails and stray neighbor pixels in the run row; ground registration
and proportions also vary. Do not use this candidate in the live course.

Capture provenance: local Unity rendering of the generated sprites on 2026-10-23,
no additional generator, under `ASSET-LICENSE.md` where rights exist. The plain dark
backdrop supports edge inspection; it does not establish light-background, small
window, grayscale or motion acceptance. Append this capture provenance to
`ASSETS.md` along with the character entry at integration.
