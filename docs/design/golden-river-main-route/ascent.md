# Golden River ascent — implementation and review

The user approved bringing vertical exploration, precision landings and two moving supports into Level 1 on 2026-11-04. This deliberately advances the moving-support introduction from the later campaign proposal. No additional movement ability is required.

The original introduction ends at x=425 and is preserved. Its short former continuation is replaced by fifteen terrace groups and a high overlook. Group widths are individually authored. Contacts rise from the river to 12.6 world units. Checkpoint 1 remains at (420, 0.52); checkpoint 2 and the provisional exit move to the overlook. The later final sequence remains unfinished. The target is 110–140 seconds for a first successful ascent including orientation, not a measured result or the completed 4–6-minute level.

A horizontal support appears in group seven, a vertical support in group eleven. Both have a five-second sine cycle and five-unit-wide landings. Standing riders receive displacement without adding launch velocity. Pause freezes motion; retry/restart resets phase. New supports and catch terraces use one-way collision. Short lower terraces offer another attempt after selected missed landings. The moving supports have non-colliding guides.

CatCamera retains intro framing, then uses a one-unit vertical dead zone and smooth follow. Its scale and zoom stay unchanged. Foreground river remains world-space. Far scenery moves less vertically than the camera. The new fortress is one independently anchored, supported hillside painting.

Preview arguments are `--preview-view level-one` for the full route and `--preview-view level-one-ascent` for a direct checkpoint start. Direct start activates checkpoint 1 for retry. It is a development convenience, not first-player evidence.

Ownership: this branch owns ascent geometry, moving-support displacement and the ascent camera corridor. PR #61 still owns water recovery and its controller/camera handoff. No CatController edits are needed here. Reconcile the camera/course overlap before readiness. Issue #71 has no native blocked-by relationships at the 2026-11-04 check. Open PRs were #77, #61 and #53. No serialized scene or package changes are intended.

Review must record successful ascent time, total retry time, falls and catches by group, platform wait time, unclear landings and retry cost. Check 960x540/640x360, grayscale, ascent, descending falls, reverse travel and camera reset. Static captures do not establish motion quality. Human timing remains pending.

## Local validation
All 32 scoped Unity EditMode tests passed with a graphics device on 2026-11-04. Actual CatController/Physics2D traversal with both supports moving reached the provisional exit without retries. Scripted traversal was 60.40 seconds for the intro and 158.04 seconds for the whole preview, or 97.64 seconds for the ascent. These are automated reachability/pacing baselines, not first-player evidence. Standing carry, jump without a velocity boost near both reversals, pause, phase reset and camera reset passed. Six static viewpoints at two sizes and grayscale were rendered. The two support clips are scripted rider reviews, not real player-input captures. Human review, route pacing, complete final section and Windows/Linux production gates remain pending.

The compact HUD displays the current ascent attempt separately. Retry at checkpoint 1 resets that attempt timer; checkpoint 2 freezes it. Full-session elapsed time and retry count remain available in Controls.

The final local Windows development preview built successfully. Archive artwork was excluded by the packed-art audit. Final timer/panorama regression also passed all 32 scoped tests. Windows/Linux production and launcher validation remain unrun.
