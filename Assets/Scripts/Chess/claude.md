# Chess — claude.md

> 세션 시작 시 루트 CLAUDE.md → 이 파일 순서로 읽을 것

---

## 이 폴더의 역할
체스 규칙 및 기물 이동 로직 전담. 정치 시스템과 직접 통신하지 않음 (EventBus 경유).

## 폴더 구조 및 파일 목록

### Board/
| 파일 | 역할 |
|------|------|
| BoardManager.cs | 8x8 초기 배치, TryMove, CheckGameResult, 장애물 배치 |
| BoardState.cs | 체스판 상태 데이터 |
| Square.cs | 개별 칸 데이터 |

### Pieces/
| 파일 | 역할 |
|------|------|
| ChessPiece.cs | 추상 기본 클래스 |
| Pawn.cs / Rook.cs / Knight.cs / Bishop.cs / Queen.cs / King.cs | 기물별 행마법 |

### Movement/
| 파일 | 역할 |
|------|------|
| MoveGenerator.cs | 의사합법 이동 계산 (임시 행마 / 포 행마 / 장애물 처리 포함) |
| MoveValidator.cs | 합법 이동 필터링 |
| SpecialMoves.cs | 캐슬링, 앙파상, 프로모션, 이동 적용 |

### Rules/
| 파일 | 역할 |
|------|------|
| CheckDetector.cs | 체크 / 공격 감지 |
| StalemateDetector.cs | 스테일메이트 감지 |
| DrawDetector.cs | 삼수동형 / 기물부족 무승부 |

## 코드 스타일
- 네임스페이스: `MMBGame`
- MoveGenerator, CheckDetector, StalemateDetector, ObedienceSystem → 정적 호출
- 300줄 초과 시 즉시 분리

---

## ChessPiece 데이터 필드 (전체)
```csharp
string pieceName
int support
int taxPerTurn
float taxModifier
List<MovePattern> originalMovePatterns
List<MovePattern> currentMovePatterns
List<MovePattern> oneTimeMovePatterns
float rebellionWeight
int punishCount
float disposition
bool isBetrayed
bool isIntelTarget
bool isOffBoard
Square offBoardOrigin
PieceColor color  // None 포함 (무승부 표현용)
PieceType type    // Barricade, Trebuchet 포함
```

---

## 작업 완료 목록

### 페이즈 1 (2026-04-05)
- PieceEnums, MovePattern, Move, Square, BoardState
- ChessPiece(추상), Pawn, Rook, Knight, Bishop, Queen, King
- MoveGenerator(의사합법), SpecialMoves(이동 적용), MoveValidator(합법 필터)
- CheckDetector, StalemateDetector, DrawDetector(삼수동형·기물부족)
- BoardEvaluator(공격자/수비자 수, 기물 가치)
- BoardManager(8x8 배치, TryMove, CheckGameResult), GameManager(싱글턴)
- 구현된 체스 규칙: 기본 행마, 폰 2칸 전진, 앙파상, 프로모션(Q/R/B/N), 캐슬링, 체크, 체크메이트, 스테일메이트, 삼수동형

### 페이즈 4 (2026-04-05)
- PieceType에 Barricade, Trebuchet 추가
- ChessPiece에 oneTimeMovePatterns 추가
- MovePattern에 isCannon 추가
- MoveGenerator에 임시 행마 / 포 행마 / 장애물 차단 처리 추가
- Barricade, Trebuchet 클래스 구현
- BoardManager에 장애물 배치 메서드 추가

### 시스템 2 (2026-04-05)
- BoardManager 이동 시도 전 ObedienceSystem 판정 연동

---

## 해야 할 작업

- [ ] 페이즈 6: 기물 성격 트리거 연결 (각 기물 클래스에 성격 메서드 추가)
  - Pawn: 물자 지원, 자치권 요구
  - Kingside Knight: 용병술, 하극상
  - Queenside Knight: 칭송, 발작성 돌격
  - Kingside Bishop: 병법, 장부조작
  - Queenside Bishop: 기사단 증원(상시), 십일조(상시)
  - Kingside Rook: 긴급피난, 긴급한 피난
  - Queenside Rook: 무역 제시, 군자금 요청
  - Queen: 모함
  - 불명예 상태 성격: 기획 예정
