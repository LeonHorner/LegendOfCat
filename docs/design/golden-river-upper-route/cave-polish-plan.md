# Enclosed riverbank cave redesign

## Status and authority

Planning revision, 2026-11-10. The user's latest in-game review rejects the open platform hall, the entry's distance from the approved reference, exposed texture boundaries, rectangular water cutoffs, loose painted specks and the floating fountain. This plan supersedes the current dry-descent composition. Existing traversal tests establish prototype reachability, not visual acceptance. No implementation or art acceptance is implied by this document.

Retain the approved entry and niche reference images in this directory. Preserve the obvious main route, visible hazardous river outside two small dry shore connections, unchanged movement and camera scale, main checkpoint ownership, a concealed local catch, a separate return and an empty future collectible anchor. No new ability, enemy, collectible, save format, package or rendering pipeline is needed. PR #79 remains draft.

### Forward destination correction

The user explicitly rejects a circular detour returning close to its entrance. The cave must be a forward alternative through the level, emerging substantially later along the main route. The existing rejoin at ascent platform 14 is superseded. Survey a later broad landing and the intervening main-route checkpoints before fixing the new endpoint; preserve mandatory checkpoint progression and do not bypass checkpoint 2 or the finale. Choose the exit for meaningful visible forward progress, then fit the cave's length and pacing to that connection. No exact platform or distance is approved yet.

The main travel direction underground is forward, with descent, short local bends, fountain discovery and an ascending exit near the far end. Do not spend most traversal time going away from the destination and doubling back. The reversible entry and short local catch recovery are intentional exceptions; they do not define the main route. The second small dry shore opening must move with the later exit, retaining dangerous visible water throughout the intervening main route.

## Diagnosis

| Current problem | Structural cause | Required replacement |
| --- | --- | --- |
| Open platform hall | A long tread array is dressed against a large rectangular rear wall | Author a connected air corridor enclosed by solid floor, ceiling and walls |
| Entry unlike reference | Broad floating recovery artwork, isolated rock arch, exposed sky between pieces | Two massive riverbank rock bodies framing a narrow descending cleft with attached steps |
| Straight cuts at top, bottom and sides | Source rectangles terminate inside the complete moving-camera envelope | Natural exposed contours; continuation edges embedded into adjoining rock beyond the envelope |
| Rectangular water ends and visible river cross-section | Coverage cuts are exposed without a physical bank/riverbed covering the join | Jointly authored water, bank, shaft and riverbed contours with painted shore contacts |
| Floating ledges and ruin | Independent quads and transparent padding are positioned by image centers | Registered rock-supported ledges, a measured ruin base, buried footing and contact shadow |
| Repeated empty space | Long horizontal sequences provide timing without enclosure or spatial reveals | Short chambers, bends, overhangs and changing floor/ceiling distances |
| Visual noise | Isolated generated flecks, repeated modules and inconsistent source scale | Clean cutouts, bounded repetition and a consistent painted stone scale |

## Spatial composition

1. **Riverbank entry.** Keep main-route contact edges and safe passage. Extend the relevant banks into complete rock masses. The reference's narrow split and small lower lip must read at the normal gameplay zoom. The existing broad recovery collider must be explicitly surveyed: integrate it into a believable landing/pocket, never advertise a narrow visual ledge over an unexplained wide standable surface. Re-route the first descent around it if necessary. Do not move main-route collisions to imitate the concept silently.
2. **Enclosing descent.** The first bend leads beneath an actual overhang. Successive attached ledges and overlapping side walls close the outside view through world geometry. The entrance remains reversible. No full-width river band or sky slit is visible once behind the bank.
3. **Short upper passage.** Replace the long exposed shelf chain with a few compact connected pockets. Walking sections initially target 2.5–3.5 world units of clearance; jump bays target 4–5.5, subject to the actual motor and collider sweep. Each required landing is visible from its takeoff. Low roofs may shape the view but cannot cause unannounced head impacts.
4. **Risky cleft.** One shaft provides a deliberate contrast to the enclosed passage. Its narrow target is readable. The catch approximately six units below is hidden by the takeoff camera and/or real rock, then revealed in time during the fall. Its return rejoins the approach without turning into a second long route. Other deep failures return to the last main checkpoint.
5. **Fountain chamber.** Approach through a narrow bent corridor, then open into an initial 10–14-unit-wide, 7–9-unit-high chamber. These are composition targets, not frozen collision dimensions. Show side walls, floor and ceiling as one enclosure. Place the unique ruin on a continuous supported floor; reserve open space beside it. Return steps remain to the side and do not cross the fountain artwork.
6. **Later ascending exit.** After substantial forward travel, use a changing-height side corridor to emerge on a much later broad main-route landing. Local bends provide enclosure without forming a large return loop. Daylight appears through progressively larger fixed openings. Reveal the later main route, hazardous water and destination before the final jump. The former nearby rejoin at platform 14 is not the target.

Target direct play remains approximately 60 seconds, with up to 90 for a short exploration. The rejected layout measured roughly 70 seconds in the scripted pilot; its long lower traverse and horizontal return must be reconsidered rather than preserved as a timing requirement. Report human and scripted times separately.

## Geometry, artwork and water contract

- Author the playable air corridor, adjoining rock volumes, shore apertures and failure spaces together in a compact route definition. The existing broad dry rectangle is not a finished cave boundary.
- Distinguish solid gameplay rock from recessed non-colliding lining. Foreground rock must not occupy traversable space without matching collision; background details must not resemble false landings.
- Attach ledges to the adjoining wall shape. Their contact spans, visible undersides, cliff fronts and headroom must agree. Preserve movement and the tested reach margin; solve accidental ceiling strikes with geometry, not control changes.
- Keep the existing shared spatial source for hazard queries and coverage masks, but replace its rough contours with the actual banks and interior. Water above the cave remains hazardous. Do not introduce a horizontal-coordinate water exemption or player-dependent reveal.
- Water cutoffs must terminate behind opaque bank/riverbed rock. Cave ceiling and dry lining must overlap structurally so neither the horizontal river bottom nor a straight rear-wall top can be exposed during descent or return.
- Build the river-to-cave cross-section first in simple geometry. Check where the water surface, solid bank, dry shaft and enclosed room meet before painting that intersection.
- Every image has an explicit role: exposed natural contour, hidden continuation edge, contact baseline and intended world scale. Match real source alpha to these anchors; do not use the quad's bounding box as a footing.
- Continue wall, roof and foundation art beyond the complete camera envelope, including maximum jump, reverse look-ahead, falls, catch recovery and respawn. A fixed overlap distance alone does not prove coverage. Any remaining edge inside that envelope must be a deliberate natural rock contour.
- Recess the fountain's measured base slightly into the floor, add restrained contact shading and join roots/rubble to the ground. The recent baseline adjustment still needs visual verification.

## Asset work

Audit every existing source before reusing it. Retain the unique ruin, quiet rear-rock texture and useful vegetation/effects where they fit. Do not preserve a fixed source count at the expense of the silhouette.

| Production batch | Planned work |
| --- | --- |
| Entry | Two matched bank/cliff bodies with a narrow split and attached lower contact; two shore joins fitted to the real water contours |
| Cave enclosure | A roof family, two turning wall forms and a chamber enclosure, all with hidden continuation edges |
| Walkable edges | Rework the three tread sizes as wall-attached stone lips with consistent stone/grass scale; avoid the repeated floating scalloped underside |
| Fountain | Reuse the unique ruin after baseline verification; integrate footing, floor, roots and one contact-shadow treatment |
| Cleanup | Remove isolated alpha components and floating yellow/green flecks from affected sources through the image authoring workflow; validate transparency against light and dark backdrops |

This is an initial production scope of roughly eleven replacement/reworked structural sources, refined after geometry approval. Large wall bodies need native proportions or shaped texture mapping rather than stretched sprite pillars. Do not repeat or mirror the unique ruin or recognizable rooted feature. Keep source prompts, dates, generator/model disclosure, license and creative decisions in ASSETS.md. Archive superseded active assets through Unity, preserving metadata.

## Lighting and camera

Retain the existing unlit shader approach. Paint cool indirect illumination into enclosed passage rock, darker solid undersides, restrained contact shadows and readable landing rims. The rear wall remains quieter than the gameplay plane; avoid uniformly bright moss on every interior edge.

Warm light enters the fountain chamber through a fixed side fissure with a plausible view to the valley side, not through the river floor. Compose the aperture, beam, lit fountain and floor patch together. Keep the beam behind Mika and landing edges and confine sparse paused dust to its volume. The risky shaft receives no catch-light or reward sparkle. No bloom, neon outline, entrance flash or renderer switching.

Keep constant zoom and Mika size, the existing vertical dead zone and smoothed facing look-ahead. Derive corridor framing from authored room bounds. Verify ordinary jumps remain calm, the catch is initially concealed, room architecture fits, and the rejoin is visible before commitment. Fix world coverage before adding any camera constraint; never hide an artwork seam with an arbitrary camera snap.

## Work sequence and review gates

1. **Survey and rejection record.** Map every supplied screenshot to a route/camera position. Mark the existing open-hall evidence as rejected. Measure bank/recovery collision, source alpha baselines and full camera envelopes. Keep useful motor, recovery, pause and water tests.
2. **Connected cave blockout.** Replace the long platform hall with the enclosed route and real roof/wall collision. Prove forward traversal, entry reversal, jumps, falls, catch return and rejoin using real physics. Tune duration through decisions and compact routing.
3. **One finished entry slice.** Fully integrate banks, two lower ledges, water, roof and lining. Produce matching-reference framing and actual fixed-zoom gameplay captures. Check both directions, jump apex and descent. Do not mass-produce the remaining artwork while the entry still looks unlike the reference.
4. **One finished underground slice.** Finish the risky cleft and the fountain approach/chamber, including edges, footing, light source and side return. Review the actual rendered game, not a new stand-alone concept image.
5. **Complete the connecting route.** Extend the established stone language, enclosure, vegetation and return. Remove repetitive voids, unsupported slabs and visible texture repetition.
6. **Full visual and technical pass.** Record the entire level with and without the detour and a deliberate catch fall at normal speed. Inspect 960×540, 640×360 and grayscale; retain camera-position evidence for all turns, jumps and transitions.
7. **Reviewable delivery.** Build and open a fresh preview with verified footing/edges. Update PR #79 and provenance. Keep it draft through player/art review; afterward integrate current main normally, coordinate the water integration with PR #61 and obtain exact-head final validation. No merge or release is authorized.

## Acceptance checklist

- Entry reads as the approved narrow rock cleft at ordinary gameplay scale; the main path remains the obvious choice.
- Exit is substantially farther along the main route; the cave produces meaningful forward progress instead of a loop back near the entry. Mandatory checkpoints and finale progression remain valid.
- The underground route consistently has visible enclosing roof/wall/floor relationships; only intentional shafts/chambers open up.
- No source rectangle, straight rear-wall boundary, exposed river cross-section, rectangular water end, sky leak or unsupported background panel is visible at any tested camera extreme, above or below.
- No decorative specks, foreground alpha halos, mismatched stone scale, floating fountain or repeated detached slab chain remains.
- The fountain has verified ground contact and an unobstructed composition; light has a physical side opening and the future item space stays empty.
- Necessary landings are visible; actual colliders match the foreground stone; no surprise ceiling collisions or false landings occur.
- Water on the main route remains visible and dangerous between the two small openings, including above the interior.
- Catch concealment/reveal, return, checkpoints, pause, retry, restart, main-route bypass and unchanged renderer activation all pass again for the new geometry.
- Compare imported texture memory and actual player frame time on the same machine/settings. The previous optimized texture audit was about 38 MiB before new replacement art; keep the additional imported-texture target near 128 MiB and verify again. Do not substitute Editor render timing for player frame rate.
- Human visual/traversal review, current Unity tests, both platform player/launcher builds, package checks and merge-ready validation remain explicit completion gates; previous prototype results cannot approve this redesign.
