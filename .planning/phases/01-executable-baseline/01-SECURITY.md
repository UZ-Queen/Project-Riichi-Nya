---
phase: 01
slug: executable-baseline
status: verified
threats_open: 0
asvs_level: 1
created: 2026-09-02
---

# Phase 01 — Security

> Phase 1 계획의 STRIDE 등록부를 ASVS L1 깊이로 구현·테스트·UAT 증거와 대조했다.

---

## Trust Boundaries

| Boundary | Description | Data Crossing |
|----------|-------------|---------------|
| Git 및 로컬 증거 경계 | 변경 가능한 작업 트리에서 기준 태그와 검증 원장을 보존한다. | Git ref, 커밋, XML·빌드·GUI 증거 |
| Unity 입력 및 세션 경계 | 키보드·버튼 입력이 솔로 세션 정책과 종료 경로로 전달된다. | 포기·취소·버림·호출 의도 |
| 씬 직렬화 및 생명주기 경계 | GUID·씬 참조·활성화 주기가 컨트롤러와 매니저를 연결한다. | MonoBehaviour 참조, 이벤트 구독 |
| 테스트 및 플레이어 저장 경계 | EditMode fixture가 실제 플레이어 저장 경로를 일시적으로 보호하고 복원한다. | `yaml.json`, 백업, 원래 없음 마커 |

---

## Threat Register

| Threat ID | Category | Component | Severity | Disposition | Mitigation | Status |
|-----------|----------|-----------|----------|-------------|------------|--------|
| T-01-01 | Tampering | `portfolio-baseline` tag | high | mitigate | annotated tag와 정확한 peeled commit을 검증하고 강제 이동하지 않는다. | closed |
| T-01-02 | Denial of Service | seeded trace loop | medium | mitigate | 고정 action cap과 최초 불일치 진단을 사용하며 trace 4/4가 통과했다. | closed |
| T-01-03 | Tampering | build/output paths | medium | mitigate | 고정 씬과 `Builds/phase1`, `Temp/phase1` 경로만 사용한다. | closed |
| T-01-04 | Repudiation | evidence summary | medium | mitigate | 명령·커밋·seed·raw 경로와 승인 경계를 `01-BASELINE.md`에 기록한다. | closed |
| T-01-20 | Tampering | Player intent routing | medium | mitigate | `ForfeitRequested`를 마작 호출 enum과 분리하고 단일 매니저 경로로 연결한다. | closed |
| T-01-21 | Denial of Service | Same-frame input | medium | mitigate | Escape를 먼저 처리하고 즉시 반환하는 회귀가 lifecycle XML에 통과했다. | closed |
| T-01-22 | Tampering | Unity asset rename | medium | mitigate | `.cs`와 `.meta` GUID 및 실제 씬 참조를 검증했다. | closed |
| T-01-23 | Tampering | Modal policy | high | mitigate | 매니저가 표시 전에 모달 상태와 입력 권한을 동기적으로 설정한다. | closed |
| T-01-24 | Elevation of Privilege | Confirm/cancel buttons | medium | mitigate | 실제 씬에서 버튼 경로가 하나이며 Cancel이 기본 선택임을 검증했다. | closed |
| T-01-25 | Denial of Service | DOTween overlay transition | medium | mitigate | 입력 차단은 tween과 독립적이고 타이머는 계속 동작한다. | closed |
| T-01-26 | Tampering | Renamed UI asset | medium | mitigate | UI asset GUID·씬 직렬화·Unity 컴파일을 검증했다. | closed |
| T-01-27 | Tampering | Renamed manager asset | medium | mitigate | 매니저 GUID와 runtime caller 이전을 검증했다. | closed |
| T-01-28 | Denial of Service | Enable/disable subscriptions | high | mitigate | `OnEnable`/`OnDisable` 대칭성과 반복 root 주기 handler 수를 검증했다. | closed |
| T-01-29 | Tampering | Prior-session callbacks | high | mitigate | 교체·종료 전 timer/round handler를 분리하고 늦은 callback이 무효임을 검증했다. | closed |
| T-01-30 | Repudiation | Test evidence | high | mitigate | 분리된 Unity XML에서 trace 4/4와 lifecycle 15/15를 정확히 검증했다. | closed |
| T-01-31 | Spoofing | Player process | medium | mitigate | 동일 PID 전체 흐름을 UAT에서 승인했다. | closed |
| T-01-32 | Repudiation | GUI evidence | high | mitigate | 관찰 전 PASS를 금지했고 최종 D-13을 UAT에서 승인했다. | closed |
| T-01-33 | Tampering | User-owned dirty files | medium | mitigate | 계획·실행·UAT 커밋을 좁게 제한하고 기존 dirty 파일을 보존했다. | closed |
| T-01-34 | Repudiation | trace diagnostic | medium | mitigate | 길이 불일치가 기존 최초 불일치 formatter를 거치는 테스트가 통과했다. | closed |
| T-01-35 | Tampering | `FinalizeGame` record value | medium | mitigate | TimeExpired 기록을 표시·저장 전에 적용하고 Forfeit no-save를 byte 수준으로 검증했다. | closed |
| T-01-36 | Tampering | restart selection presentation | medium | mitigate | 실제 씬 root/controller/view 재시작에서 6번 패 하나만 선택됨을 XML과 UAT로 확인했다. | closed |
| T-01-37 | Repudiation | Unity test evidence | high | mitigate | XML 존재·양수 discovery·정확한 이름·0 non-pass를 fail-closed로 요구한다. | closed |
| T-01-38 | Tampering | lifecycle test persistence | high | mitigate | 기존 저장은 같은 디렉터리 백업으로 이동하고 원래 없음은 마커로 기록하며 두 복구 분기를 검증했다. | closed |
| T-01-39 | Denial of Service | interrupted fixture cleanup | high | mitigate | SetUp/TearDown의 멱등 복구와 복원 완료 전 authoritative artifact 보존을 검증했다. | closed |
| T-01-40 | Information Disclosure | local save backup | low | accept | 동일 사용자 권한의 로컬 저장 옆에만 임시 보관하고 복구 후 제거하며 네트워크·커밋에 노출하지 않는다. | closed |
| T-01-41 | Repudiation | final Unity evidence | high | mitigate | 최종 4+15 XML 게이트와 독립 복구 재검증이 통과했고 누락 증거는 PASS로 취급하지 않는다. | closed |

*Status: open · closed · open — below high threshold (non-blocking)*

---

## Accepted Risks Log

| Risk ID | Threat Ref | Rationale | Accepted By | Date |
|---------|------------|-----------|-------------|------|
| R-01-01 | T-01-40 | 백업은 동일 로컬 사용자 권한 아래에만 존재하고 자동 복구 후 제거된다. Phase 1 범위에서 별도 암호화·저장 프레임워크는 위험보다 복잡성이 크다. | Phase 1 plan disposition | 2026-09-02 |

---

## Security Audit Trail

| Audit Date | Threats Total | Closed | Open | Run By |
|------------|---------------|--------|------|--------|
| 2026-09-02 | 26 | 26 | 0 | Codex ASVS L1 artifact audit |

---

## Sign-Off

- [x] All threats have a disposition (mitigate / accept / transfer)
- [x] Accepted risks documented in Accepted Risks Log
- [x] `threats_open: 0` confirmed
- [x] `status: verified` set in frontmatter

**Approval:** verified 2026-09-02
