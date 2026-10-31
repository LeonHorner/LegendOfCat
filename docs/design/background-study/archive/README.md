# Background study review archive

Archived on 2026-10-30 for issue #50. These immutable copies preserve representative visual experiments even after a squash merge. This is a curated review archive, not a backup of every historical Unity project or every preview. Dates below are source commit dates, not verified screenshot capture dates. Rejected experiments remain evidence, not accepted production art.

## Major iterations

| Commit date | Stage | Review outcome / purpose | Evidence |
| --- | --- | --- | --- |
| 2026-10-24 | 01-comparison | Initial A/B/C comparison. Superseded: blurry contours and insufficient lower framing. | [candidate-0.png](01-comparison/candidate-0.png), [candidate-1.png](01-comparison/candidate-1.png), [candidate-2.png](01-comparison/candidate-2.png), [candidate-0.mp4](01-comparison/candidate-0.mp4) |
| 2026-10-25 | 02-crisp-cutouts | Crisper layer contours and fixed 16:9 framing; intermediate experiment. | [candidate-0.png](02-crisp-cutouts/candidate-0.png), [candidate-0-small-grayscale.png](02-crisp-cutouts/candidate-0-small-grayscale.png) |
| 2026-10-25 | 03-extended-terrain | Longer route, textured terrain, hanging foliage and deeper caps. Superseded: excessive depth and high foreground water. | [candidate-0.png](03-extended-terrain/candidate-0.png), [extended-course-0.png](03-extended-terrain/extended-course-0.png) |
| 2026-10-27 | 04-flat-river-framing | Flatter tops and revised river framing. Water still appeared disconnected. | [candidate-0.png](04-flat-river-framing/candidate-0.png) |
| 2026-10-27 | 05-connected-river | One connected distant/near river painting. Rejected direction: grounded terrain and moving scenery could not stay visually joined. | [candidate-0.png](05-connected-river/candidate-0.png), [candidate-0.mp4](05-connected-river/candidate-0.mp4) |
| 2026-10-28 | 06-water-visibility | Restored visible foreground water and attempted anchoring. Platform bases still appeared suspended. | [river-contact-0.png](06-water-visibility/river-contact-0.png) |
| 2026-10-28 | 07-painted-formations | Whole painted rock formations replaced repeated wall textures. Shore integration remained unresolved. | [river-contact-0.png](07-painted-formations/river-contact-0.png), [play-jump.png](07-painted-formations/play-jump.png) |
| 2026-10-28 | 08-ground-contact | Joined cliff banks and adjusted Mika contact; follows lighting correction 8a2f777. Intermediate, not final acceptance. | [river-contact-0.png](08-ground-contact/river-contact-0.png), [play-ground-contact.png](08-ground-contact/play-ground-contact.png) |
| 2026-10-29 | 09-world-shore | World-locked shore with parallax behind shallow suspended shelves. Rejected connected foreground/background architecture. | [river-contact-0.png](09-world-shore/river-contact-0.png), [play-jump.png](09-world-shore/play-jump.png), [world-shore-review.mp4](09-world-shore/world-shore-review.mp4) |
| 2026-10-29 | 10-separated-depth | Independent recessed valley, gameplay plane and foreground river. Direction selected; comparison options subsequently removed. | [candidate-0.png](10-separated-depth/candidate-0.png), [candidate-1.png](10-separated-depth/candidate-1.png), [candidate-2.png](10-separated-depth/candidate-2.png), [separated-depth-review.mp4](10-separated-depth/separated-depth-review.mp4) |
| 2026-10-30 | 11-fixed-subtle | Fixed Subtle distance and first foreground water animation. User reported insufficient visible motion. | [subtle-distance.png](11-fixed-subtle/subtle-distance.png), [water-animation.mp4](11-fixed-subtle/water-animation.mp4) |
| 2026-10-30 | 12-visible-flow | Visible game-clock water flow, moving foam and fixed rocks. Supersedes earlier weak animation. | [subtle-distance.png](12-visible-flow/subtle-distance.png), [water-flow.mp4](12-visible-flow/water-flow.mp4) |
| 2026-10-30 | 13-running-waves | Latest implementation: grounded running increases flow and wave activity with smooth transitions. Clip demonstrates water states; it is not proof of player-input behavior. | [water-running.mp4](13-running-waves/water-running.mp4) |

Minor corrections are grouped with their parent approach: metadata normalization (7b299a0), river lighting (8a2f777), ground contact (2d36f2c), and deterministic jump test scheduling (703d9ea). The latter changes test scheduling rather than art direction.

## Original user feedback

The seven uploads below are byte-identical copies. Capture dates are unknown; sequence follows the conversation. Original upload filenames and SHA-256 checksums are in [manifest.json](manifest.json). The first is the existing generated concept reference, not a new generated asset.

- [01-concept-reference](feedback/01-concept-reference.png): Concept reference reposted while discussing high water, excessive depth and Mika placement.
- [02-disconnected-water](feedback/02-disconnected-water.png): Foreground and background water look separate.
- [03-disconnected-water-jump](feedback/03-disconnected-water-jump.png): Second view of disconnected water during a jump.
- [04-floating-bases](feedback/04-floating-bases.png): Platforms appear to float and foreground water is missing.
- [05-shader-line-endings](feedback/05-shader-line-endings.png): Unity reports mixed line endings in StudyRiver.shader.
- [06-grounded-parallax-conflict](feedback/06-grounded-parallax-conflict.png): Grounded bases conflict with moving background scenery; clipped secondary trees.
- [07-suspended-platform-shape](feedback/07-suspended-platform-shape.png): Suspended platforms are too tall and need distinct undersides; architecture questioned.

## Local motion capture

The 291 local numbered images are represented by three 97-frame videos and nine exact keyframes (start, midpoint, end). Playback is 5 fps (19.4 seconds), matching the period exporter setting. Exact capture revision and date could not be verified; these are not performance recordings or evidence of the current implementation. MP4 encoding is lossy. Original frames remain untouched locally and are inventoried with checksums; they are deliberately not all committed.

- [Mode 0 video](local-capture/motion-0.mp4): [start](local-capture/motion-0-00.png), [midpoint](local-capture/motion-0-48.png), [end](local-capture/motion-0-96.png).
- [Mode 1 video](local-capture/motion-1.mp4): [start](local-capture/motion-1-00.png), [midpoint](local-capture/motion-1-48.png), [end](local-capture/motion-1-96.png).
- [Mode 2 video](local-capture/motion-2.mp4): [start](local-capture/motion-2-00.png), [midpoint](local-capture/motion-2-48.png), [end](local-capture/motion-2-96.png).

## Source material and provenance

Active source paintings remain in [BackgroundStudy resources](../../../../Assets/Game/Resources/BackgroundStudy/). Six superseded originals and their metadata are retained in [BackgroundStudyArchive](../../../../Assets/Game/Art/BackgroundStudyArchive/), outside Resources; no source pixels or evidence were discarded. Historical Git paths in the manifest describe capture-time provenance, not current asset locations. See [ASSETS.md](../../../../ASSETS.md) for generator, licensing and human creative decisions. Existing prompt records remain alongside the [current study notes](../README.md): cutouts, terrain/corners, depth/foliage, connected river, terrain modules, world shore and separated depth. Current runtime settings and known limitations belong to those current notes.

The manifest records full source revisions and paths for historical Git media, exact copy checksums, and the local frame inventory. No new artwork was generated, retouched or recomposed for this archive. Historical clips preserve the settings of their source revision and should be read alongside the stage description; they do not imply acceptance or current test success.
