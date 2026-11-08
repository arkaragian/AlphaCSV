#!/bin/sh
set -eu

repo_root="$(CDPATH= cd "$(dirname "$0")" && pwd -P)"
cd "$repo_root"
results_root="$repo_root/TestResults"
mkdir -p "$results_root"
run_dir="$(mktemp -d "$results_root/coverage.XXXXXX")"

dotnet build "$repo_root/TestAlphaCSV/TestAlphaCSV.csproj" --target:Rebuild
dotnet test --project "$repo_root/TestAlphaCSV/TestAlphaCSV.csproj" \
    --no-build \
    --results-directory "$run_dir" \
    --coverlet \
    --coverlet-output-format cobertura

report_found=0
report_paths=
for report in "$run_dir"/coverage.cobertura*.xml; do
    if [ -f "$report" ]; then
        printf 'Coverage report: %s\n' "$report"
        report_found=1
        if [ -z "$report_paths" ]; then
            report_paths="$report"
        else
            report_paths="$report_paths;$report"
        fi
    fi
done

if [ "$report_found" -eq 0 ]; then
    printf 'Coverage report was not generated in %s\n' "$run_dir" >&2
    exit 1
fi

dotnet tool restore --tool-manifest "$repo_root/dotnet-tools.json"
dotnet reportgenerator \
    "-reports:$report_paths" \
    "-targetdir:$repo_root/coverage-report" \
    "-reporttypes:Html" \
    "-filefilters:-*/obj/*"

printf 'HTML report: %s\n' "$repo_root/coverage-report/index.html"
