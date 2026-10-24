# Complete-character run study

Historical rejected study. Its preparation script and referenced refined source
now live under ../archive/ with their original repository-relative paths. The
accepted live run remains the original four-frame sequence, not this study.

Generated on 2026-10-24 using built-in OpenAI image generation (exact model ID
not exposed), referencing `identity-reference.png`, a crop of the earlier
`Assets/Game/Art/Mika/mika-movement-refined-source.png`. Terms: ASSET-LICENSE.md
where rights exist. No external artwork was imported.

Human direction: restore substantial paws and natural complete-cat anatomy;
replace the rejected cutout rig with complete-character frames. The generation
requested sixteen chronological right-facing trot/run poses in a 4-by-4 sheet,
stable torso/head proportions and markings, opposing diagonal support pairs,
low recovery arcs, equal scale, transparent background and no motion blur,
crossfade, separated parts or image deformation.

`run-source-v1.png` preserves the generated result. The preparation script
`tools/prepare-mika-run-study.py` performs alpha cleanup and translation only,
registering the upper silhouette's rightmost point and lowest opaque sole.
It does not correct anatomy or prove that each support foot is planted.
`registration.json` records these translations. `run-registered.png` is the
registered atlas. Normal preview uses 40 ms per pose; slow preview uses 120 ms.
Intermediate PNGs are regenerated into ignored Logs/mika-run-study.

## Review status

**Study only; not accepted or integrated.** Paws have more volume than the rejected
rig, but torso length, shoulder position and tail still vary. Several poses repeat
similar strides instead of providing even chronological spacing. The support
exchange and final-to-first transition need redrawing. Registration aligns the
lowest sole to the review floor; it does not establish correct gait contacts.
Increasing playback speed or generating interpolated frames would hide these
problems rather than resolve them.

Review `run-normal.gif`, `run-slow.gif` and `review-contact-sheet.png` before any
gameplay integration. A coherent run cycle remains unfinished. Jump/landing
replacement, Unity integration and issue #10 acceptance remain outstanding.
