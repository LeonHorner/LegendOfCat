# Cave terrain rebuild

The owner rejected the terrain-correction candidate on 2026-11-14. Its screenshots and prior 236-test result are historical evidence, not acceptance of this rebuild.

## Scope and sequencing

Replace the disconnected, stretched terrain with one authored solid model. Keep the approved fountain position and the distant rejoin. The new deeper fissure starts after the fountain. The return climbs through three short reversals rather than a long flat lane.

The first visual review gate is the actual level entrance, both shore connections, and the first fully enclosed passage. Complete that slice before producing the remaining final artwork. Physics validation alone does not open this gate. Keep PR #79 as a draft; do not merge or release.

## Current implementation

The owner subsequently removed the hard memory cap and prioritized Golden River startup performance. Memory measurements remain informational and must not limit artwork quality.

- `CaveTerrainLayout` supplies both rock meshes and source polygons for a static manual `CompositeCollider2D`. Moving main supports remain independent.
- Opaque rock bodies extend to Y -42. Vegetation is a separate layer and cannot punch holes through solid rock. Exposed contact spans exclude buried portions and merge adjoining coplanar crowns.
- Two additional descending ledges give the first closed passage real roof thickness and jump headroom. The formerly inconsistent passage/fountain join shares the same Y -12 contact.
- The post-fountain fissure descends to Y -21.3. The local concealed catch is at Y -26.5; the cave death boundary is -34.
- The full forward path, the local catch return and the flat passage join in both directions passed the isolated actual-controller geometry runner. These are scripted physics results, not human timing or artwork acceptance.
- Water uses separate permanent shore envelopes. Surface-crossing tests use the player's foot location. Opaque foundations cover the lower water boundary. Camera depth and zoom stay fixed.
- Six native entrance modules preserve a common rounded slate material at six-unit source frames. Unity import preserves the 1254x1254 original resolution. Vegetation and painted edge detail remain over opaque bodies; internal material joins do not change world opacity.
- The permanent riverbed uses a small irregular solid contour, shared by collision and rendering, so its contact with water is not a straight image-bottom rectangle.
- The generated water/dry mask keeps 4096 horizontal samples and 256 vertical samples; its render boundary stays buried in solid rock. Continuous hazard queries still use the original world polygons. See the latest evidence for informational texture memory measurements.

## Startup performance

The same editor review construction on the same hardware measured 3237.0 ms before precomputation and 802.1 ms after it, about 75% less startup time. Cave terrain/detail setup dropped from 2009.0 to 62.8 ms and coverage setup from 370.4 to 3.8 ms. These are single editor observations with authoring assets already imported, not a cold-player benchmark or an FPS claim. Raw timing lines are retained in `rebuild-evidence/startup-before.txt` and `startup-after.txt`.

Seven linear data masks are now prepared in the editor and loaded as Resources when the level opens. A source SHA-256 signature invalidates the preparation cache after changes to the route, solid contours or generator. Build checks require the prepared masks. A regression compares every saved pixel with the current procedural generator; gameplay danger queries still use the world polygons. Shared prepared textures are not destroyed when one course instance closes. Fragment clipping also rejects pixels outside native detail frames before the expensive stone sampling.

The rebuilt Windows preview measured 486.8 ms for the first `OpenLevelOne` construction in a fresh player process, with OS and graphics-driver caches left intact. This is the method invoked by the Golden River menu button. Its result is recorded separately from the editor comparison.

## Entrance review candidate

The actual approach, descent into the first closed passage and reverse entrance are recorded in [rebuild evidence](rebuild-evidence/README.md), alongside color and grayscale camera poses. Evaluate the wet shoreline contacts, permanent throat, contact correspondence and material consistency in motion before extending this art treatment. The first slice is a review candidate, not owner-accepted final art.

The remaining full-route artwork is intentionally not presented as finished. The stronger climbing return currently uses the same shared solid model as a tested geometry prototype. Earlier evidence does not substitute for this revision's checks.

## Later production work after the first visual review

Finish the fissure, rooted catch stairs, fountain lighting and climbing chambers with the same accepted module language. Finish the far exit mouth and its shores; prove it remains present when reversing. The 27 unused rejected terrain Resources have been archived through Unity with metadata preserved; the standalone accepted study inputs remain active. Track memory and frame rate without the superseded hard texture cap, and keep human and scripted duration separate. Complete exact-head Windows/Linux player, launcher, package and merge-ready validation only when the feature is ready.

## Coordination

This work owns the cave layout, cave rendering, cave shaders and cave review helpers. PR #82 also touches `MovementCourse.cs` and `ASSETS.md`; this rebuild has not changed `MovementCourse.cs`. Any later shared entry-point integration must preserve both features. Asset provenance changes are append-only because the existing `ASSETS.md` contains legacy non-UTF-8 bytes.
Startup measurement also touches `StarterScreen.OpenLevelOne`; its gameplay behavior and UI text are unchanged.

## Rejected new art experiment

A built-in image-generation trial on 2026-11-14 produced a 1254x1254 upper-rock module with inset contact and a different outer silhouette. It was not imported into Resources: at the requested eight-unit frame it misses the 192 px/unit requirement, and its inset surface would recreate the contact mismatch. Generator/model revision was not disclosed. No generated candidate is accepted solely because the image looks attractive in isolation.
