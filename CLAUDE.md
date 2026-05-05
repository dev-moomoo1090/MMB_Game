# MMB_Game — 최상위 워크플로우

---

## 워크플로우 오케스트레이션

### 1. 플랜 모드 기본값
- 사소하지 않은 모든 작업(3단계 이상 또는 아키텍처 결정)에서 플랜 모드 진입
- 뭔가 잘못되면, 즉시 STOP하고 다시 계획 수립
- 단순 구현뿐 아니라 검증 단계에도 플랜 모드 사용
- 모호함을 줄이기 위해 사전에 상세한 스펙 작성

### 2. 서브에이전트 전략
- 메인 컨텍스트 윈도우를 깔끔하게 유지하기 위해 서브에이전트를 적극 활용
- 리서치, 탐색, 병렬 분석은 서브에이전트에 위임
- 복잡한 문제일수록 서브에이전트에 더 많은 연산 투입
- 집중 실행을 위해 서브에이전트 하나당 하나의 작업만 담당

### 3. 자기개선 루프
- 사용자로부터 수정사항이 생기면: tasks/lessons.md에 패턴 업데이트
- 같은 실수를 방지하는 규칙을 스스로 작성
- 실수율이 떨어질 때까지 이 교훈들을 끊임없이 반복 개선
- 세션 시작 시 해당 폴더의 claude.md 복습

### 4. 완료 전 검증
- 작동한다는 것을 증명하지 않고는 절대 작업 완료로 표시하지 말 것
- 관련이 있다면 메인과 변경사항 간의 동작 차이를 diff로 확인
- 스스로에게 물을 것: "시니어 엔지니어가 이걸 승인할까?"
- 테스트 실행, 로그 확인, 정확성 입증

### 5. 우아함 추구 (균형 있게)
- 사소하지 않은 변경 시: 잠깐 멈추고 "더 우아한 방법이 있나?" 자문
- 수정이 억지스럽게 느껴진다면: "지금 아는 모든 것을 바탕으로, 우아한 해결책을 구현하라"
- 단순하고 명확한 수정에는 이를 생략 — 과도한 설계 금지
- 제출 전에 본인 작업을 스스로 검토

### 6. 자율적 버그 수정
- 버그 리포트를 받으면: 그냥 고칠 것. 일일이 안내 요청 금지
- 로그, 에러, 실패하는 테스트를 직접 확인한 후 해결
- 사용자의 컨텍스트 전환 불필요
- 따로 지시받지 않아도 실패하는 CI 테스트를 찾아서 수정

---

## 작업 관리

1. **먼저 계획:** tasks/todo.md에 체크 가능한 항목으로 계획 작성
2. **계획 검증:** 구현 시작 전 확인
3. **진행 추적:** 진행하면서 완료 항목 표시
4. **변경사항 설명:** 각 단계마다 고수준 요약 제공
5. **결과 문서화:** tasks/todo.md에 리뷰 섹션 추가
6. **교훈 기록:** 수정 후 tasks/lessons.md 업데이트

---

## 핵심 원칙

- **단순함 우선:** 모든 변경을 최대한 단순하게. 코드 영향 최소화.
- **게으름 금지:** 근본 원인을 찾을 것. 임시방편 수정 금지. 시니어 개발자 기준.
- **최소 영향:** 변경은 꼭 필요한 것만. 버그 유입 금지.
- **파일 분리 원칙:** 한 파일에는 하나의 기능만.

---

## 매 세션 시작 시 필수 순서

1. 이 파일 읽기 (워크플로우 파악)
2. 작업할 폴더의 `claude.md` 읽기 (해당 폴더 상세 맥락)
3. AGENTS.md 읽고 Codex에 명령 전달 (Codex 사용 시)

---

## 전체 아키텍처

```
Scripts/
├── Core/
│   ├── GameManager.cs
│   ├── TurnManager.cs
│   ├── EventBus.cs
│   ├── PlayerState.cs
│   ├── KingStateEvaluator.cs
│   ├── KingStateEffectApplier.cs
│   └── RebellionSystem.cs
│
├── Chess/
│   ├── Board/
│   │   ├── BoardManager.cs
│   │   ├── BoardState.cs
│   │   └── Square.cs
│   ├── Engine/
│   │   ├── StockfishBridge.cs
│   │   └── FenConverter.cs
│   ├── Pieces/
│   │   ├── ChessPiece.cs
│   │   ├── PieceEnums.cs
│   │   ├── Pawn.cs / Rook.cs / Knight.cs / Bishop.cs / Queen.cs / King.cs
│   │   └── Obstacle.cs
│   ├── Movement/
│   │   ├── Move.cs
│   │   ├── MovePattern.cs
│   │   ├── MoveGenerator.cs
│   │   ├── MoveValidator.cs
│   │   └── SpecialMoves.cs
│   └── Rules/
│       ├── CheckDetector.cs
│       ├── StalemateDetector.cs
│       ├── DrawDetector.cs
│       ├── BoardEvaluator.cs
│       ├── ObedienceSystem.cs
│       └── AutonomousMovement.cs
│
├── Politics/
│   ├── PoliticsManager.cs
│   ├── Fiscal/
│   │   ├── FiscalAction.cs
│   │   ├── TaxActions.cs
│   │   ├── ResourceActions.cs
│   │   ├── LoanAction.cs
│   │   └── DeploymentActions.cs
│   ├── Military/
│   │   ├── MilitaryAction.cs
│   │   ├── MilitaryManager.cs
│   │   ├── EnhancementActions.cs
│   │   ├── ObstacleActions.cs
│   │   ├── BombardAction.cs
│   │   └── DelayedEffect.cs
│   └── Political/
│       ├── PoliticalAction.cs
│       ├── PoliticalManager.cs
│       ├── PropagandaAction.cs
│       ├── CivilAidAction.cs
│       ├── ReconAction.cs
│       ├── VisitAction.cs
│       ├── FineAction.cs
│       ├── ExecutionAction.cs
│       ├── AgitationAction.cs
│       ├── ManipulationAction.cs
│       ├── BribeAction.cs
│       ├── IntelAction.cs
│       └── AssassinationAction.cs
│
└── UI/                 ← 페이즈 7 (미구현)
```

---

## 개발 진행 현황

- [x] 페이즈 1: 체스판 & 기물 기초 + 모든 체스 규칙
- [x] 페이즈 2: 턴 시스템
- [x] 페이즈 3: 재정 카테고리
- [x] 페이즈 4: 군사 카테고리
- [x] 페이즈 5: 정치 카테고리
- [ ] 페이즈 6: 기물 성격 시스템 ← **현재 진행 중**
- [ ] 페이즈 7: UI & 밸런싱

**마지막 업데이트:** 2026-04-29
