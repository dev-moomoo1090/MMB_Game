# Core — claude.md

> 세션 시작 시 루트 CLAUDE.md → 이 파일 순서로 읽을 것

---

## 이 폴더의 역할
게임 전체 흐름을 조율하는 최상위 시스템. 구체적인 게임 로직은 보유하지 않음.

## 파일 목록
| 파일 | 역할 |
|------|------|
| GameManager.cs | 싱글턴. 모든 Manager 참조 및 초기화 |
| TurnManager.cs | GamePhase(PoliticsPhase/ChessPhase) 열거형, StartGame(), EndPhase() |
| EventBus.cs | 싱글턴. 컴포넌트 간 이벤트 통신 전용 |

## 코드 스타일
- 네임스페이스: `MMBGame`
- Manager는 조율만 — 로직 작성 금지
- 300줄 초과 시 즉시 분리

---

## 작업 완료 목록

### 페이즈 2 (2026-04-05)
- EventBus 싱글턴: OnPhaseChanged / OnTurnChanged / OnGameEnded 이벤트
- TurnManager: GamePhase 열거형, StartGame(), EndPhase()
- GameManager: TurnManager 참조 추가, Start()에서 StartGame() 호출

### 시스템 4 (2026-04-05)
- GameManager에 KingStateEffectApplier 참조 및 초기화 추가

### 시스템 5 (2026-04-05)
- GameManager에 AutonomousMovement 참조 및 초기화 추가

### EventBus 이벤트 목록 (현재)
- OnPhaseChanged, OnTurnChanged, OnGameEnded
- OnMovementRefused
- OnRebellionTriggered, OnDefectionTriggered
- OnActionExecuted, OnIntelligenceGathered, OnReconResult
- OnAutonomousMoveExecuted
- 예약 제거 이벤트 (MilitaryManager용)

---

## 해야 할 작업

- [ ] 페이즈 6: 기물 성격 트리거 이벤트 EventBus에 추가 (성격 시스템 구현 시)
- [ ] 페이즈 7: UI Manager 연결
