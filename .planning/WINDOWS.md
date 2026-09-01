---
schema_version: 1
open_count: 0
waived_count: 0
fixed_count: 3
total_count: 3
last_updated: 2026-09-01T08:51:45.413Z
---

# Broken Windows Ledger

> Cross-phase defect register. With `workflow.windows_enforce` enabled, `/gsd-ship` blocks while `open_count > 0`.
> Waive with `gsd-tools windows waive <id> "<reason>"` (reason required).
> Mark fixed with `gsd-tools windows fixed <id>`.

| id | phase | kind | file | line | description | status | reason | recorded_at | resolved_at |
|----|-------|------|------|------|-------------|--------|--------|-------------|-------------|
| 1 | 01 | deviation | Assets/Scripts/SoundArchive/SoundArchive.cs |  | Empty serialized sound groups blocked the Windows Player build and were safely ignored | fixed |  | 2026-09-01T08:50:57.013Z | 2026-09-01T08:51:36.630Z |
| 2 | 01 | deviation | Assets/Editor/Phase1Build.cs |  | BuildReport required durable output before explicit Unity Editor exit | fixed |  | 2026-09-01T08:50:57.454Z | 2026-09-01T08:51:44.975Z |
| 3 | 01 | deviation | Assets/Scenes/SampleScene.unity |  | Four new Korean labels required the existing Korean TMP atlas to avoid missing glyph boxes | fixed |  | 2026-09-01T08:50:57.898Z | 2026-09-01T08:51:45.413Z |

````json
[
  {
    "id": 1,
    "kind": "deviation",
    "phase": "01",
    "file": "Assets/Scripts/SoundArchive/SoundArchive.cs",
    "line": null,
    "description": "Empty serialized sound groups blocked the Windows Player build and were safely ignored",
    "status": "fixed",
    "reason": "",
    "recorded_at": "2026-09-01T08:50:57.013Z",
    "resolved_at": "2026-09-01T08:51:36.630Z"
  },
  {
    "id": 2,
    "kind": "deviation",
    "phase": "01",
    "file": "Assets/Editor/Phase1Build.cs",
    "line": null,
    "description": "BuildReport required durable output before explicit Unity Editor exit",
    "status": "fixed",
    "reason": "",
    "recorded_at": "2026-09-01T08:50:57.454Z",
    "resolved_at": "2026-09-01T08:51:44.975Z"
  },
  {
    "id": 3,
    "kind": "deviation",
    "phase": "01",
    "file": "Assets/Scenes/SampleScene.unity",
    "line": null,
    "description": "Four new Korean labels required the existing Korean TMP atlas to avoid missing glyph boxes",
    "status": "fixed",
    "reason": "",
    "recorded_at": "2026-09-01T08:50:57.898Z",
    "resolved_at": "2026-09-01T08:51:45.413Z"
  }
]
````
