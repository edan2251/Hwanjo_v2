"""Record one explicitly requested Unity invocation; never declare tests/builds passed."""
import argparse
import datetime
import json
from pathlib import Path
import re
import subprocess
import sys

parser = argparse.ArgumentParser()
parser.add_argument("--run-dir", required=True)
parser.add_argument("--label", required=True)
parser.add_argument("--player", help="Run an existing workspace player instead of the Editor")
parser.add_argument("unity_args", nargs=argparse.REMAINDER)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
run = Path(args.run_dir).resolve()
run.relative_to(root / "artifacts")
run.mkdir(parents=True, exist_ok=True)
editor = Path(r"C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe")
command = [str(editor), "-projectPath", str(root / "Hwanjo_v2_Test"), "-logFile", str(run / (args.label + ".editor.log"))]
if args.player:
    player = (root / args.player).resolve()
    player.relative_to(root / "Builds")
    command = [str(player), "-logFile", str(run / (args.label + ".editor.log"))]
command += args.unity_args[1:] if args.unity_args[:1] == ["--"] else args.unity_args
record = {"command": command, "startedAt": datetime.datetime.now().astimezone().isoformat(), "exitCode": None}
print("Starting Unity: " + args.label, flush=True)
with (run / (args.label + ".stdout.log")).open("wb") as out, (run / (args.label + ".stderr.log")).open("wb") as err:
    process = subprocess.Popen(command, cwd=root / "Hwanjo_v2_Test", stdout=out, stderr=err, creationflags=subprocess.CREATE_NO_WINDOW)
    record["pid"] = process.pid
    (run / (args.label + ".result.json")).write_text(json.dumps(record, indent=2), encoding="utf-8")
    record["exitCode"] = process.wait()
record["finishedAt"] = datetime.datetime.now().astimezone().isoformat()
record["redactedFields"] = 0
for path in run.glob(args.label + ".*.log"):
    raw = path.read_bytes()
    clean, count = re.subn(rb"(?im)^([ \t]+(?:(?:External )?(?:Session|Correlation|Machine) Id|Id|Serial(?: number)?|License key|API key|Password):[ \t]*)[^\r\n]+", rb"\1[REDACTED]", raw)
    if clean != raw:
        path.write_bytes(clean)
    record["redactedFields"] += count
(run / (args.label + ".result.json")).write_text(json.dumps(record, indent=2), encoding="utf-8")
print(json.dumps(record, indent=2), flush=True)
sys.exit(record["exitCode"])
