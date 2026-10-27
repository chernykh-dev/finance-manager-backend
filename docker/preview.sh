#!/usr/bin/env bash
# Manages per-PR preview stands on the VPS (see docs/preview-stands.md).
#
#   preview.sh up <pr> [--reset] [--db-copy]   create or update stand fmb-<pr>
#   preview.sh down <pr>                       remove stand with its data
#
# --reset    drop the stand's database before starting
# --db-copy  fill a fresh database with a copy of the main one (fmb-db container)
set -euo pipefail

MAX_STANDS="${MAX_STANDS:-3}"
NETWORK=fmb-preview
MAIN_DB_CONTAINER=fmb-db

usage() {
  echo "usage: $0 up <pr> [--reset] [--db-copy] | $0 down <pr>" >&2
  exit 2
}

[[ $# -ge 2 && "$2" =~ ^[0-9]+$ ]] || usage
command="$1"
export PR_NUMBER="$2"
shift 2

project="fmb-pr-${PR_NUMBER}"
cd "$(dirname "$0")"
compose=(docker compose -p "$project" -f docker-compose.preview.yaml)

docker network inspect "$NETWORK" >/dev/null 2>&1 || docker network create "$NETWORK" >/dev/null

case "$command" in
  up)
    reset=false
    db_copy=false
    for arg in "$@"; do
      case "$arg" in
        --reset) reset=true ;;
        --db-copy) db_copy=true ;;
        *) usage ;;
      esac
    done

    # Other stands currently running (this one doesn't count: updating it is always allowed).
    # grep exits 1 on no match: guard it, or pipefail + set -e would abort when no stands exist.
    others=$(docker ps --format '{{.Label "com.docker.compose.project"}}' \
      | { grep -E '^fmb-pr-[0-9]+$' || true; } | sort -u | { grep -cvxF "$project" || true; })
    if (( others >= MAX_STANDS )); then
      echo "Stand limit reached: ${others}/${MAX_STANDS} other stands are running." >&2
      exit 3
    fi

    if [[ "$reset" == true ]]; then
      "${compose[@]}" down -v --remove-orphans
    fi

    fresh_db=false
    docker volume inspect "${project}_pgdata" >/dev/null 2>&1 || fresh_db=true

    "${compose[@]}" pull --quiet
    "${compose[@]}" up -d --wait db redis

    if [[ "$db_copy" == true && "$fresh_db" == true ]]; then
      # Before the app starts: the app then applies this PR's migrations on top of the copy.
      echo "Copying main database into ${project}..."
      docker exec "$MAIN_DB_CONTAINER" pg_dump -U postgres -d fmb-db --no-owner --no-privileges \
        | "${compose[@]}" exec -T db psql -q -v ON_ERROR_STOP=1 -U postgres -d fmb-db >/dev/null
    fi

    "${compose[@]}" up -d --wait app
    docker image prune -f >/dev/null
    echo "Stand ${project} is up: http://fmb-${PR_NUMBER}.yourflow.pro"
    ;;

  down)
    [[ $# -eq 0 ]] || usage
    "${compose[@]}" down -v --remove-orphans --rmi all
    echo "Stand ${project} removed"
    ;;

  *)
    usage
    ;;
esac
