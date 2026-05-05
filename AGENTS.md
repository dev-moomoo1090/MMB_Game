# MMB_Game - Codex 작업 가이드

## 작업 시작 전 필수
1. 이 파일 전체를 읽고 기존 코드 구조 파악
2. Claude로부터 받은 명령 범위 확인
3. 해당 범위 외 파일 수정 금지

---

## 코드 스타일
- **네임스페이스**: `MMBGame`
- **클래스명**: PascalCase
- **변수명**: camelCase
- **상수**: UPPER_SNAKE_CASE
- **주석 없음** (코드 자체로 의미 전달)
- **접근제한자 명시 필수**: public / private / protected 항상 표기

---

## 작업 규칙

### 코드 작성
- 파일 분리 원칙 준수 (CLAUDE.md 참고)
- 300줄 초과 시 즉시 분리
- 새 파일 생성 시 올바른 폴더에 배치
- Manager 클래스는 조율만, 로직은 별도 파일

### Unity 에디터 작업
- 코드 작성 후 Unity MCP로 씬 적용
- 컴파일 에러 확인 후 다음 단계 진행
- 플레이 모드 테스트로 동작 확인

### 디버깅
- 에러 발생 시 2회까지 스스로 디버깅
- 2회 실패 시 Claude에게 보고 (에러 메시지, 시도한 방법 포함)

### 완료 후
- 이 파일 "컴포넌트 연결 구조" 섹션 업데이트
- Claude에게 변경사항 보고:
  - 생성/수정한 파일 목록
  - 구현한 기능 요약
  - 미완료 사항 (있다면)

---

## 기물 데이터 구조
모든 ChessPiece는 다음 데이터를 보유:
- `string pieceName` - 기물 이름
- `int support` - 지지도
- `int taxPerTurn` - 턴당 세금
- `List<MovePattern> originalMovePatterns` - 기존 행마법
- `List<MovePattern> currentMovePatterns` - 현재 행마법 (정치행동으로 변경 가능)
- `float rebellionWeight` - 반란 가중치
- `PieceColor color` - 백/흑
- `PieceSide side` - 없음/퀸사이드/킹사이드
- `PieceType type` - 기물 종류

---

## 체스 규칙 (페이즈 1 필수 구현)
- 기본 행마법: 폰, 룩, 나이트, 비숍, 퀸, 킹
- 폰 첫 이동 두 칸 전진
- 앙파상
- 프로모션 (폰이 끝줄 도달 시)
- 캐슬링 (킹사이드 / 퀸사이드)
- 체크 / 체크메이트
- 스테일메이트
- 삼수동형무승부 (동일 국면 3회 반복)

---

## 컴포넌트 연결 구조

| 클래스 | 참조하는 클래스 | 방식 |
|--------|----------------|------|
| BoardManager | BoardState, MoveValidator, CheckDetector, SpecialMoves, DrawDetector, ObedienceSystem, EventBus, BoardPieceVisuals, PieceSetupDefinition, PieceSideResolver | 직접 호출 / SerializeField |
| BoardPieceVisuals | BoardState, PieceSetupDefinition, ChessPiece, BoardPieceVisual, SpriteRenderer | 직접 읽기 / 생성 |
| BoardPieceVisual | BoardPieceVisuals, ChessPiece, SpriteRenderer, BoxCollider2D | 직접 호출 |
| PieceSetupDefaults | PieceSetupDefinition, MovePattern | 정적 호출 |
| PieceSideResolver | PieceType | 정적 호출 |
| ChessPiece | MovePattern, PieceSetupDefinition, PieceSideResolver | 직접 적용 |
| MoveValidator | MoveGenerator, CheckDetector, SpecialMoves | 정적 호출 |
| MoveGenerator | BoardState, ChessPiece (서브클래스) | 직접 읽기 |
| SpecialMoves | BoardState, ChessPiece 서브클래스 생성 | 직접 변경 |
| ObedienceSystem | ChessPiece | 정적 호출 |
| CheckDetector | BoardState | 직접 읽기 |
| BoardEvaluator | BoardState | 직접 읽기 |
| StalemateDetector | CheckDetector, MoveValidator | 정적 호출 |
| DrawDetector | BoardState | 직접 읽기 |
| GameManager | BoardManager, TurnManager, PoliticsManager, MilitaryManager, PoliticalManager, RebellionSystem, KingStateEffectApplier, AutonomousMovement | SerializeField |
| RebellionSystem | BoardManager, BoardState, EventBus | 직접 호출 |
| KingStateEffectApplier | BoardManager, PoliticsManager, KingStateEvaluator, BoardEvaluator, EventBus | 직접 호출 |
| AutonomousMovement | BoardManager, MoveValidator, CheckDetector, BoardEvaluator, SpecialMoves, EventBus | 직접 호출 |
| TurnManager | EventBus | 직접 호출 |
| EventBus | (없음) | 싱글턴 |
| PoliticsManager | PlayerState, FiscalAction 서브클래스, BoardManager, EventBus | 직접 호출 |
| FiscalAction (서브클래스) | ChessPiece, PlayerState, PoliticsManager, BoardManager | 직접 변경 |
| MilitaryManager | BoardManager, MilitaryAction 서브클래스, EventBus | 직접 호출 |
| MilitaryAction (서브클래스) | ChessPiece, MilitaryManager, BoardState, CheckDetector | 직접 변경 |
| PoliticalManager | BoardManager, PoliticsManager, PoliticalAction 서브클래스, EventBus | 직접 호출 |
| PoliticalAction (서브클래스) | ChessPiece, PoliticalManager, PlayerState, BoardState, EventBus | 직접 변경 |
| KingStateEvaluator | PlayerState, BoardState, ChessPiece | 정적 호출 |

---

## 구현 완료 목록

### 페이즈 1 (2026-04-05)
- **데이터**: PieceEnums, MovePattern, Move, Square, BoardState
- **기물**: ChessPiece (추상), Pawn, Rook, Knight, Bishop, Queen, King
- **이동**: MoveGenerator (의사합법 이동), SpecialMoves (이동 적용), MoveValidator (합법 이동 필터)
- **규칙**: CheckDetector (체크/공격 감지), StalemateDetector, DrawDetector (삼수동형·기물부족무승부)
- **평가**: BoardEvaluator (공격자/수비자 수, 기물 가치)
- **관리**: BoardManager (8x8 초기 배치, TryMove, CheckGameResult), GameManager (싱글턴)

### 구현된 체스 규칙
- 기본 행마법 (폰/룩/나이트/비숍/퀸/킹)
- 폰 첫 이동 두 칸 전진
- 앙파상
- 프로모션 (Q/R/B/N 선택)
- 캐슬링 (킹사이드 / 퀸사이드)
- 체크 감지
- 체크메이트 감지
- 스테일메이트 감지
- 삼수동형무승부

### 페이즈 2 (2026-04-05)
- **EventBus**: 싱글턴, OnPhaseChanged / OnTurnChanged / OnGameEnded 이벤트
- **TurnManager**: GamePhase 열거형 (PoliticsPhase/ChessPhase), StartGame(), EndPhase()
- **GameManager**: TurnManager 참조 추가, Start()에서 StartGame() 호출
- **PieceEnums**: PieceColor에 None 추가 (무승부 표현용)

### 페이즈 3 (2026-04-05)
- **재정 상태**: PlayerState 추가, 플레이어별 gold 관리
- **기물 확장**: ChessPiece에 taxModifier, isOffBoard, offBoardOrigin, 세금/턴 리셋 메서드 추가
- **재정 행동**: FiscalAction 추상 클래스, 특세/감면, 지원/징발, 사채, 후방배치/전방배치 구현
- **관리**: PoliticsManager 추가, 턴 변경 시 기물 세금 배율 초기화, 현재 턴 플레이어 기준 재정 행동 실행

### 페이즈 4 (2026-04-05)
- **기물 확장**: PieceType에 Barricade, Trebuchet 추가, ChessPiece에 oneTimeMovePatterns 추가
- **이동 확장**: MovePattern에 isCannon 추가, MoveGenerator에 임시 행마/포 행마/장애물 차단 처리 추가
- **군사 기물**: Barricade, Trebuchet 장애물 기물 구현, BoardManager에 장애물 배치 메서드 추가
- **군사 행동**: MilitaryAction, DelayedEffect, 장애물 설치/투석/강화 행동 구현
- **관리**: MilitaryManager 추가, EventBus에 예약 제거 이벤트 추가, GameManager에 MilitaryManager 초기화 추가

### 페이즈 5 (2026-04-05)
- **기물 확장**: ChessPiece에 isBetrayed, isIntelTarget 추가
- **이벤트 확장**: EventBus에 OnActionExecuted / OnIntelligenceGathered / OnReconResult 추가
- **정치 행동**: PoliticalAction 추상 클래스, 정찰/매수/정보/암살/선전/대민지원/방문/벌금/처형/선동/여론조작 구현
- **관리**: PoliticalManager 추가, GameManager에 PoliticalManager 초기화 추가, PoliticsManager/MilitaryManager 성공 행동 이벤트 발행 추가

### 데이터 구조 보강 (2026-04-05)
- **기물 확장**: ChessPiece에 punishCount, disposition 추가
- **플레이어 상태 확장**: PlayerState에 honor, goldPerTurn, AddHonor() 추가
- **상태 평가**: PieceEnums에 KingState 추가, KingStateEvaluator 생성
- **턴 수입 처리**: PoliticsManager에서 턴 시작 시 goldPerTurn 자동 지급

### 시스템 2 (2026-04-05)
- **행마 거부**: ObedienceSystem 추가, BoardManager 이동 시도 전 지지도/성향 기반 거부 판정 및 EventBus OnMovementRefused 이벤트 발행

### 시스템 3 (2026-04-05)
- **반란/배신**: RebellionSystem 추가, 체스 페이즈 시작 시 현재 턴 기물 대상 반란/배신 판정 및 EventBus OnRebellionTriggered / OnDefectionTriggered 이벤트 발행

### 시스템 4 (2026-04-05)
- **왕 상태 효과 적용**: KingStateEffectApplier 추가, 정치 페이즈 시작 시 KingState별 지지도/명예/수입/반란 가중치 효과 적용
- **플레이어 상태 확장**: PlayerState에 OnHonorChanged 콜백 추가, 독재 상태의 명예 감소 연동 지원
- **턴 리셋 보강**: ChessPiece.ResetTurnModifiers()에 rebellionWeight 초기화 추가
- **관리**: GameManager에 KingStateEffectApplier 참조 및 초기화 추가

### 시스템 5 (2026-04-05)
- **자율 이동**: AutonomousMovement 추가, 체스 페이즈 시작 시 킹사이드 룩이 체크 상태가 아니면서 공격받고 있으면 50% 확률로 가장 안전한 합법 위치로 자율 이동
- **이벤트 확장**: EventBus에 OnAutonomousMoveExecuted 추가
- **관리**: GameManager에 AutonomousMovement 참조 및 초기화 추가

### 정치행동 사양 반영 보강 (2026-04-29)
- **기물 상태 확장**: ChessPiece에 acceptWeight 추가, BoardState에 offBoardPieces와 GetAllPieces() 추가
- **재정 행동 보강**: 사채 후보 선정/상환 대기, 후방배치 보드 제거, 전방배치 원위치 및 3x3/5x5/7x7 복귀 탐색 구현
- **군사 행동 보강**: 바리케이드/트레뷰셋 설치 조건, 고유 개수 제한, 자기 정치 턴 기준 지연 처리, 투석 전방 제한 및 중복 발동 제한 구현
- **강화 행동 보강**: 전초기지/신의 힘/기적/군법면제의 체크 상태 제한과 다음 군사 페이즈 적용 처리
- **정치 행동 보강**: 선전/선동 무입력 비례 효과, 복지/여론조작 세금 비례 효과, 벌금/처형 형벌 카운터 공식 반영
- **정치 행동 추가**: 제후국, 심문 구현
- **후방배치 연동**: 후방배치 기물도 정치 효과/정보 초기화/반란 판정 대상에 포함, 반란 시 전방 복귀 처리

### 보드 기물 비주얼/설정 보강 (2026-05-06)
- **기물 설정**: PieceSetupDefinition 추가, 인스펙터에서 색상/종류별 기본/선택 프리팹 또는 스프라이트, 초기 세금, 초기 지지도, 기본 행마법 설정 지원
- **기본값 생성**: PieceSetupDefaults 추가, BoardManager 컨텍스트 메뉴에서 표준 12기물 정의 자동 생성 지원
- **비주얼 생성**: BoardPieceVisuals 추가, 보드 타일 위치에 맞춰 SpriteRenderer 기물 자동 생성 및 보드 상태 변경 시 동기화
- **기물 선택 비주얼**: BoardPieceVisual 추가, 기물 클릭 시 선택 프리팹/스프라이트로 전환하고 이전 선택 기물은 기본 비주얼로 복귀
- **복제 보강**: ChessPiece Clone 시 커스텀 행마법/상태가 시뮬레이션에 유지되도록 CopyStateTo 추가
- **사이드 구분**: PieceSide와 PieceSideResolver 추가, 킹/퀸은 None, 폰/룩/나이트/비숍은 퀸사이드/킹사이드 정의로 분리
