## Feature
Describe the complete user-visible result in English. One feature may cover multiple issues.

## Related issues
List all related issues. Use "Closes #123" only for completed issues.

## Remaining work
List incomplete acceptance criteria. Keep the PR as a draft while the feature is unfinished.

## Documentation
Describe documentation and asset-provenance updates.

## Final validation
Do not run tests after every commit. Once the feature is complete, run Actions > Release with mode=prepare_merge and this PR number.
Record the workflow URL and validated head SHA. Never claim skipped or failed checks passed.
New commits or a changed base require final validation again.

## Merge requirements
- English Conventional Commit title describing the full feature.
- Complete feature, updated documentation, and resolved conversations.
- Successful merge-ready status on the exact revision and an up-to-date branch.
- Squash merge only after explicit authorization; never force-push.
