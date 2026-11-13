# Hidden garden gameplay assets - 2026-11-09

The owner authorized implementing the concealed-garden direction and opening a
playable preview. This revision remains in draft for actual gameplay review.

Original built-in OpenAI image-generation assets, exact model identifier not
exposed; transparent output was requested and preserved. Only the project's
garden-concept-v2-storyboard.png was supplied as visual reference. No external
artwork or licensing dependency. Existing ASSET-LICENSE.md terms apply.

- garden-court-v1.png: source exec-532bd831-9750-4258-9c77-56ed421e2676.png.
  A complete tree, dry fountain and low enclosing walls/arcade share a flat lawn
  and solid foundation. The lawn is registered at 13.2% of source height; its
  standable span has a separate solid collider. Decorative fountain/tree/walls
  are behind Mika, with no implied new ability or reward.
- garden-wall-v1.png: source exec-45f2c1fa-2f5b-4060-b2af-4b2865f917a8.png.
  Ordinary ivy-covered violet masonry with a narrow shadowed crack. No painted
  gameplay steps; actual treads receive their own collision/paint registration.

Human decisions: reject the exposed shelf chain; choose a ruin garden; keep the
entrance inconspicuous with a small clue; completely remove the main route from
the garden/passage camera view; reveal the garden only after exploration. Codex
authored the leftward climbing approach, secluded courtyard and higher returning
arcade. No main-route collision array, checkpoint, ability or save format changes.

Exact prompts:

## Courtyard

Production 2D game environment cutout on genuine transparent background, no sky, no landscape, no text, no cat. Use the RIGHT panel of reference solely for style. A complete secluded overgrown ruined courtyard: magnificent gnarled tree with warm golden canopy, old DRY stone fountain embraced by roots to its left, violet masonry enclosing low broken walls, teal ivy, ferns and small pale flowers, a ruined side arcade on right. Strict side-on flat 2D elevation, no perspective ground plane. Broad horizontal garden base at the bottom, FLAT crisp continuous playable grass/stone contact line across the entire width at exactly 15 percent of image height from bottom; solid violet foundation fills bottom 15 percent, slight irregular bottom edge. All tree/fountain/decoration sits above this common baseline, no foliage across the walkable baseline. Leave comfortable open walking space and reveal tree/fountain clearly. Width about twice height, warm directional light, original hand-painted style matching reference, complete tree crown inside image with margin, no cropping. This is an actual game backdrop/platform illustration, not a concept board, not multiple panels, no floating extra ledges, no ground shadow outside asset. Isolated transparent alpha everywhere outside the complete tree, walls, arcade and garden foundation.

## Wall

Production 2D game cutout on genuine transparent background. Use LEFT and MIDDLE panels for violet masonry, golden moss and teal ivy style. One ordinary old ruin wall section, strict side-on flat elevation. Roughly square. A modest irregular top silhouette and deep solid masonry continuing all the way to the straight bottom edge. An inconspicuous narrow vertical dark crack near the RIGHT lower quarter, partly covered by hanging teal ivy; subtle golden moss, no grand arch, no obvious doorway, no sign, no glow. The crack is shadowed stone, not a transparent hole. NO steps or platforms painted into wall, no free foliage specks, no fountain/tree/cat/sky/landscape. Complete isolated wall asset, little golden foliage at its top. Stone joints intricate but quiet, warm edge lighting, muted dark blue-violet faces. Genuine transparency outside the wall silhouette, entirely opaque stone center. Designed as a background wall behind collision treads.
