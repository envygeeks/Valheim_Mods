#!/bin/bash
set -e

# --
# Make sure steam is launch
# it'll fail otherwise!
# --

exec /usr/bin/arch -x86_64 \
  "$(GamePath)/run_with_bepinex.sh" "$@"
