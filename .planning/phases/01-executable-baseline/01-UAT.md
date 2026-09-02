---
status: complete
phase: 01-executable-baseline
source: [01-01-SUMMARY.md, 01-02-SUMMARY.md, 01-03-SUMMARY.md, 01-04-SUMMARY.md, 01-05-SUMMARY.md, 01-06-SUMMARY.md, 01-07-SUMMARY.md]
started: 2026-09-02T08:00:43.3700925Z
updated: 2026-09-02T08:06:17.0808829Z
---

## Current Test

[testing complete]

## Tests

### 1. 고정 시드 솔로 추적 재현
expected: 시드 1557 솔로 추적이 공개 라운드 API를 통해 재현된다.
result: pass
source: automated
coverage_id: 01-01:D1

### 2. 포기 종료 및 반복 시작 안정성
expected: 포기 확정은 저장 기록을 바꾸지 않고 한 번만 종료되며, 반복 시작은 구독과 세션 상태를 초기화한다.
result: pass
source: automated
coverage_id: 01-01:D2

### 3. 초기 Windows Player 전체 흐름
expected: 같은 Windows Player PID에서 솔로 시작, 한글 포기 확인창, 결과 화면, 메뉴 복귀, 새 게임 재시작이 이어진다. 확인창이 열린 동안 타이머는 계속 움직이고 일반 패 입력은 차단되며, 한글 글리프가 깨지지 않고 재시작 입력은 한 번만 반응한다.
result: pass
coverage_id: 01-01:D3

### 4. PlayerHandView 표시 책임
expected: 기존 씬 계층을 복제하지 않고 PlayerHandView가 패 생성, 손패와 쯔모패 표시, 행동 표시, 선택 강조를 담당한다.
result: pass
source: automated
coverage_id: 01-02:D1

### 5. 포기 입력 의도 분리
expected: PlayerHandController가 별도 ForfeitRequested를 발생시키고 매니저는 한 번만 구독하며 PlayerCallType에는 마작 행동만 남는다.
result: pass
source: automated
coverage_id: 01-02:D2

### 6. 솔로 UI 표시 책임
expected: SoloScoringUIController가 기존 솔로 출력 참조와 일반 패널 동작을 소유하면서 이전 스크립트 GUID를 유지한다.
result: pass
source: automated
coverage_id: 01-03:D1

### 7. 포기 오버레이 입력 정책
expected: 씬 로컬 포기 오버레이가 일반 게임 입력을 즉시 차단하고 확인과 취소를 한 번씩 전달하며 EventSystem 기본 선택은 취소 버튼이다.
result: pass
source: automated
coverage_id: 01-03:D2

### 8. 솔로 생명주기 소유자
expected: SoloScoringGameManager가 기존 직렬화 스크립트 정체성을 유지한 유일한 솔로 생명주기 및 정책 소유자다.
result: pass
source: automated
coverage_id: 01-04:D1

### 9. 모드 루트 반복 활성화 안정성
expected: 하나의 비활성 씬 루트가 솔로 활성화와 로비 해제를 제어하고 반복 주기에도 이벤트 구독 수가 정확하다.
result: pass
source: automated
coverage_id: 01-04:D2

### 10. 기준선 및 런타임 경계
expected: 기준선 태그, descriptor 일치, 시드 추적, 런타임 UnityEditor import 금지 경계가 유지된다.
result: pass
source: automated
coverage_id: 01-05:D1

### 11. 생명주기 회귀 묶음
expected: 모달, 시간초과, 구독, 로비, 재시작, 표시, 입력 순서 계약이 명명된 생명주기 회귀 테스트로 검증된다.
result: pass
source: automated
coverage_id: 01-05:D2

### 12. 8단계 동일 PID 흐름 확인
expected: 같은 Windows Player PID에서 시작, 포기 취소, 포기 확정, 로비 복귀, 재시작의 8단계 흐름이 정상이며 포커스, 타이머, 한글 결과, 중복 없는 반응이 보인다.
result: pass
coverage_id: 01-05:D3

### 13. 추적 길이 불일치 진단
expected: 짧아진 추적은 최초 분기 행동과 간결한 예상 및 실제 상태를 actual=<missing>과 함께 보고한다.
result: pass
source: automated
coverage_id: 01-06:D1

### 14. 시간초과 기록과 재시작 선택
expected: 시간초과는 같은 새 기록을 표시하고 저장하며 포기는 저장을 바꾸지 않고 실제 루트 재시작은 6번 패만 선택한다.
result: pass
source: automated
coverage_id: 01-06:D2

### 15. 4+14 자동 검증 묶음
expected: 분리된 필터 게이트에서 추적 4개와 생명주기 14개가 통과한다.
result: pass
source: automated
coverage_id: 01-06:D3

### 16. 비기본 패 선택 후 D-13 재실행
expected: 새 Windows Player의 같은 PID에서 비기본 패을 선택한 뒤 포기, 메뉴, 재시작을 수행하면 재시작 후 6번 패만 강조된다.
result: pass
coverage_id: 01-06:D4

### 17. 중단된 저장 복구
expected: 중단된 생명주기 테스트가 다음 변경 전에 백업 바이트를 정확히 복원하고 원래 저장이 없던 상태도 정리한다.
result: pass
source: automated
coverage_id: 01-07:D1

### 18. 모든 저장 변경 경로의 보호
expected: 포기와 두 시간초과 저장 경로가 하나의 내구성 있는 fixture 보호 절차를 거치면서 기존 행동 검증을 유지한다.
result: pass
source: automated
coverage_id: 01-07:D2

### 19. 4+15 최종 자동 검증 묶음
expected: 분리된 필터 게이트에서 추적 4개와 생명주기 15개가 고유 이름으로 모두 통과한다.
result: pass
source: automated
coverage_id: 01-07:D3

### 20. 최종 D-13 동일 PID 재시작 표시
expected: 새 Windows Player의 같은 PID에서 포기, 메뉴 복귀, 재시작을 완료하면 선택 표시가 6번 패 하나로 초기화된다.
result: pass
coverage_id: 01-07:D4

## Summary

total: 20
passed: 20
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none yet]
