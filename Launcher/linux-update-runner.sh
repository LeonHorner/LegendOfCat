#!/bin/sh
# Standalone POSIX supervisor: no dependency on the AppImage mount or .NET runtime.
set -eu

ready=$1
stage=$2
log=$3
launcher_pid=$4
launcher_identity=$5
game_pid=$6
game_identity=$7
appimage=$8
package=$9
shift 9
packages_dir=$1
shutdown_timeout=${2:-180}
case "$shutdown_timeout" in
    ''|*[!0-9]*) echo "Invalid handoff timeout" >&2; exit 2 ;;
esac
[ "$shutdown_timeout" -gt 0 ] && [ "$shutdown_timeout" -le 180 ] ||
    { echo "Invalid handoff timeout" >&2; exit 2; }
updater="$stage/usr/bin/UpdateNix"

# Never retain pipes or a working directory owned by the disappearing AppImage.
cd "$stage"
exec </dev/null >>"$log" 2>&1

cleanup() {
    rm -rf -- "$stage"
}
trap cleanup EXIT

process_identity() {
    [ -r "/proc/$1/stat" ] || return 1
    awk '{
        line = $0
        sub(/^.*\) /, "", line)
        n = split(line, fields, " ")
        if (n < 20 || fields[1] == "Z" || fields[1] == "X") exit 1
        print fields[20]
    }' "/proc/$1/stat" 2>/dev/null
}
alive_with_identity() {
    observed=$(process_identity "$1") || return 1
    [ "$observed" = "$2" ]
}

if [ ! -x "$updater" ] || [ ! -r "$stage/usr/bin/sq.version" ] ||
   [ ! -f "$appimage" ] || [ ! -s "$package" ] || [ ! -d "$packages_dir" ]; then
    echo "Linux update handoff: required staged files, target, or package missing"
    exit 1
fi
if ! alive_with_identity "$launcher_pid" "$launcher_identity" ||
   ! alive_with_identity "$game_pid" "$game_identity"; then
    echo "Linux update handoff: launch processes already exited before readiness"
    exit 1
fi

# Parent sends Unity the shutdown command only after this readiness acknowledgement.
printf 'ready\n' >"$ready"
echo "Linux update handoff ready; waiting for both game and entry launcher"
deadline=$(($(date +%s) + shutdown_timeout))
while alive_with_identity "$launcher_pid" "$launcher_identity" ||
      alive_with_identity "$game_pid" "$game_identity"; do
    if [ "$(date +%s)" -ge "$deadline" ]; then
        echo "Linux update handoff timed out; refusing to replace a running install"
        exit 1
    fi
    sleep 0.1
done

echo "Both processes exited; invoking staged Velopack native updater"
# No --waitPid: Velopack 1.2.161 continues after a wait timeout.
# The explicit PID-identity barrier above is fail-closed instead.
"$updater" --log "$log" apply --package "$package" \
    --rootDir "$appimage" --packageDir "$packages_dir"
echo "Staged native updater completed"
