#!/usr/bin/env python3
"""Run real Unity Test Framework on an activated local editor; never fabricate a pass."""
import argparse
import glob
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
VERSION = (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text().split(':', 1)[1].strip()


def find_editor(explicit=None):
    candidates = [explicit, os.environ.get('UNITY_EDITOR'), shutil.which('Unity'), shutil.which('unity-editor')]
    candidates.extend([
        f'/Applications/Unity/Hub/Editor/{VERSION}/Unity.app/Contents/MacOS/Unity',
        str(Path.home() / f'Unity/Hub/Editor/{VERSION}/Editor/Unity'),
        f'C:/Program Files/Unity/Hub/Editor/{VERSION}/Editor/Unity.exe',
    ])
    for base in ['/opt/Unity/Hub/Editor', '/opt/unity', str(Path.home() / 'Unity/Hub/Editor')]:
        candidates.extend(glob.glob(base + '/**/Editor/Unity', recursive=True))
    for candidate in candidates:
        if candidate and Path(candidate).is_file():
            return Path(candidate).resolve()
    return None


def read_result(path):
    """Accept a successful NUnit run only if tests were executed and none were skipped."""
    root = ET.parse(path).getroot()
    if root.tag != 'test-run':
        raise ValueError('Expected NUnit test-run root')
    total = int(root.attrib.get('total', '0'))
    passed = int(root.attrib.get('passed', '0'))
    failed = int(root.attrib.get('failed', '0'))
    skipped = int(root.attrib.get('skipped', '0'))
    inconclusive = int(root.attrib.get('inconclusive', '0'))
    result = root.attrib.get('result', '')
    if total <= 0 or passed != total or failed or skipped or inconclusive or result != 'Passed':
        raise ValueError(f'Unity tests not fully passed: result={result}, total={total}, passed={passed}, failed={failed}, skipped={skipped}, inconclusive={inconclusive}')
    return {'total': total, 'passed': passed, 'failed': failed, 'skipped': skipped, 'result': result}


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity', help='Path to an activated Unity editor executable')
    parser.add_argument('--mode', choices=['all', 'EditMode', 'PlayMode'], default='all')
    parser.add_argument('--timeout', type=int, default=1800, help='Seconds allowed per test mode')
    parser.add_argument('--output', type=Path, default=ROOT / 'TestResults' / 'local-unity')
    args = parser.parse_args(argv)
    args.output.mkdir(parents=True, exist_ok=True)
    report = {'status': 'blocked', 'requiredUnity': VERSION, 'runs': [], 'scope': 'Real Unity import and Test Framework; not manual visual/balance acceptance.'}
    editor = find_editor(args.unity)
    try:
        if editor is None:
            report['reason'] = f'Unity Editor not found. Install and activate Unity {VERSION} with Unity Hub, then use --unity or UNITY_EDITOR.'
            print('BLOCKED: ' + report['reason'], file=sys.stderr)
            return 2
        report['editor'] = str(editor)
        failed = False
        for mode in (['EditMode', 'PlayMode'] if args.mode == 'all' else [args.mode]):
            result_path = (args.output / f'{mode}.xml').resolve()
            log_path = (args.output / f'{mode}.log').resolve()
            # A previous XML file must never make a crashed editor appear successful.
            if result_path.exists():
                result_path.unlink()
            command = [str(editor), '-batchmode', '-nographics', '-projectPath', str(ROOT), '-runTests', '-testPlatform', mode, '-testResults', str(result_path), '-logFile', str(log_path)]
            print(f'Running {mode}; log: {log_path}', flush=True)
            run = {'mode': mode, 'xml': str(result_path), 'log': str(log_path)}
            try:
                proc = subprocess.run(command, timeout=args.timeout, check=False)
                run['exitCode'] = proc.returncode
                if proc.returncode != 0:
                    raise RuntimeError(f'Unity exited {proc.returncode}; inspect the log for import, compilation or licensing errors')
                run.update(read_result(result_path))
                run['status'] = 'passed'
                print(f'PASS: {mode}, {run["passed"]} tests', flush=True)
            except (OSError, ET.ParseError, ValueError, RuntimeError, subprocess.TimeoutExpired) as error:
                failed = True
                run['status'] = 'failed'
                run['reason'] = str(error)
                print(f'FAIL: {mode}: {error}', file=sys.stderr)
            report['runs'].append(run)
        report['status'] = 'failed' if failed else 'passed'
        return 1 if failed else 0
    finally:
        (args.output / 'summary.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')


if __name__ == '__main__':
    sys.exit(main())
