# Codex TODO - 기획서 변경 반영 작업 메모

작성일: 2026-05-07

이 파일은 MCP 연결이 끊긴 뒤 새 세션에서 바로 작업을 이어가기 위한 메모다.  
현재 요청은 "구현하지 말고, 다음 세션이 읽을 TODO 파일로 정리"였다.

## 중요한 전제

- 사용자가 `체스정치게임 기획서 (2).docx`를 새 기준으로 줬다.
- 기존 구현은 정치행동 버튼/실행 흐름은 대부분 연결되어 있지만, 새 기획서 기준의 수식/가중치/개체별 데이터가 정확히 맞지는 않는다.
- 보드 회전/확대 작업은 사용자가 실행 취소했다고 했으므로, 다시 요청받기 전까지 보드 뷰 조작 작업은 건드리지 말 것.
- 폰트 Tangba14 적용, 커맨드 버튼 재배치, 프로필 텍스트 자동 축소 등 이전 UI 작업은 유지된 상태일 가능성이 높다.
- 현재 작업 트리에 이미 사용자/이전 세션 변경이 많을 수 있으니, 수정 전 반드시 `git status --short`와 관련 파일 내용을 확인하고 사용자 변경을 되돌리지 말 것.

## 새 기획서에서 확인된 주요 변경점

### 1. 기본 기물 데이터

사용자가 이미지로 표 의미를 정정했다.

표는 "칸별 기본 지지도"가 아니라, 설명을 칸처럼 적어둔 **초기 기물 개체별 기본 지지도**다.

기본 가치/세금:

| 기물 | 가치 | 세금 |
| --- | ---: | ---: |
| 폰 | 1 | 20 |
| 나이트 | 3 | 30 |
| 비숍 | 3 | 30 |
| 룩 | 5 | 50 |
| 퀸 | 9 | 90 |

초기 개체별 기본 지지도:

| 개체 | 기본 지지도 |
| --- | ---: |
| a2 폰 | 45 |
| b2 폰 | 40 |
| c2 폰 | 35 |
| d2 폰 | 30 |
| e2 폰 | 70 |
| f2 폰 | 65 |
| g2 폰 | 60 |
| h2 폰 | 55 |
| a1 룩 | 30 |
| b1 나이트 | 15 |
| c1 비숍 | 45 |
| d1 퀸 | 20 |
| e1 킹 | 100 |
| f1 비숍 | 80 |
| g1 나이트 | 65 |
| h1 룩 | 70 |

중요:

- 폰이 현재 킹사이드/퀸사이드 2그룹으로만 나뉘어 있으면 안 된다.
- 폰은 8개 개체로 분리되어야 한다.
- 이 구분에 종속되는 코드도 모두 같이 바꿔야 한다.

권장 설계:

- `PieceSide`를 무리하게 A~H까지 확장하지 말고, 별도 개체 식별자를 추가하는 편이 안전하다.
- 예: `PieceLane` 또는 `PieceColumn` enum 추가.

```csharp
public enum PieceLane
{
    None,
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H
}
```

- `ChessPiece`에 `public PieceLane lane;` 추가.
- 초기 배치 시 시작 rank/file 기준으로 lane을 지정.
- `PieceSide`는 기존 퀸사이드/킹사이드 로직 호환을 위해 유지.
- 프리팹/프로필 이미지가 lane별로 없으면 기존 side별 리소스로 fallback.

관련 파일 후보:

- `Assets/Scripts/Chess/Pieces/PieceEnums.cs`
- `Assets/Scripts/Chess/Pieces/ChessPiece.cs`
- `Assets/Scripts/Chess/Board/BoardManager.cs`
- `Assets/Scripts/Chess/Board/BoardPieceSetupManager.cs`
- `Assets/Scripts/Chess/Board/BoardPieceProfileDisplay.cs`
- `Assets/Scripts/Chess/Board/BoardPieceVisuals.cs`
- `Assets/Scripts/Chess/Board/PieceSetupDefaults.cs` 또는 동등 파일
- `Assets/Scripts/Chess/Board/PieceSetupDefinition.cs`

### 2. 수락/배신/반란 공식 변경

새 기획서 공식:

- 이동수락 = `50 + 개별지지도 + 수비하는 기물 수 * 50 - 공격하는 기물 수 * 50 + 보드 전체 수락가중치 + 기타 가중치`
- 일반수락 = `50 + 개별지지도 + 보드 전체 수락가중치 + 기타 가중치`
- 배신 = `30 - 개별지지도 + 보드 전체 배신가중치 + 기타 가중치`
- 반란 = `20 - 개별지지도 + 보드 전체 반란/배신 가중치 + 기타 가중치`

현재 상태 추정:

- `ObedienceSystem`은 예전식 `refusalChance = (50 - support) * 0.5f - disposition * 0.1f` 형태.
- `RebellionSystem`도 예전식 확률 계산을 사용.
- `acceptWeight`, `rebellionWeight`, `disposition`이 섞여 있어 새 문서의 "보드 전체 가중치"와 "기타 가중치" 구조가 명확하지 않다.

권장 설계:

- `BoardState` 또는 별도 `BoardPoliticalWeights`에 전역 가중치 추가:
  - `int globalAcceptanceWeight`
  - `int globalDefectionWeight`
  - `int globalRebellionWeight`
- `ChessPiece`에는 개체별 기타 가중치 유지/정리:
  - `int acceptWeight`는 "기물별 기타 수락 가중치"로 재정의하거나 이름 변경 검토.
  - `float rebellionWeight`는 확률 배율처럼 쓰지 말고 "기타 반란 가중치"로 정수화 검토.
  - 배신 기타 가중치가 별도로 필요하면 `int defectionWeight` 추가.
- `ObedienceSystem`은 `RollMovementAcceptance` 또는 `RollMovementRefusal`처럼 의미가 명확한 API로 정리.
- 공격자/수비자 수는 이미 `BoardEvaluator.CountAttackers`, `BoardEvaluator.CountDefenders`가 있으므로 활용 가능.

관련 파일 후보:

- `Assets/Scripts/Chess/Rules/ObedienceSystem.cs`
- `Assets/Scripts/Core/RebellionSystem.cs`
- `Assets/Scripts/Chess/Board/BoardState.cs`
- `Assets/Scripts/Chess/Rules/BoardEvaluator.cs`
- `Assets/Scripts/Chess/Board/BoardManager.cs`
- `Assets/Scripts/Chess/Pieces/ChessPiece.cs`

### 3. 반란 시스템 미구현/부정확

새 기획서:

- 지지도가 하락할 때마다 반란 판정 실행.
- 반란 판정이 누적 3회 성공하면 기물의 이동 권한이 상대에게 넘어감.

현재 상태 추정:

- `RebellionSystem`은 체스 페이즈 시작 시 현재 턴 기물을 검사하는 흐름.
- 지지도 감소 시점에 중앙 이벤트로 반란 판정을 돌리는 구조가 부족함.
- 반란 성공 누적 카운트와 "이동 권한 이전"이 명확히 구현되어 있지 않음.

권장 설계:

- `ChessPiece`에 `int rebellionSuccessCount` 추가.
- 지지도 변경을 직접 필드 대입으로 하지 않고 헬퍼/API로 감싸는 방안 검토:
  - 예: `PoliticalStatService.ChangeSupport(piece, delta, reason)`
  - 단기적으로는 모든 `piece.support +=/-=` 지점을 찾아 `RebellionSystem.NotifySupportChanged(piece, delta)` 호출로 묶어도 됨.
- 반란 3회 성공 시 처리:
  - "색 자체를 바꾸는 배신"과 구분해야 한다.
  - 이동 권한만 상대에게 넘어가는 별도 상태가 필요할 수 있음.
  - 예: `PieceColor movementControllerColor` 또는 `bool isRebelControlled`.
- UI/프로필에 반란 상태를 표시할지 추후 결정.

관련 파일 후보:

- `Assets/Scripts/Core/RebellionSystem.cs`
- `Assets/Scripts/Chess/Pieces/ChessPiece.cs`
- `Assets/Scripts/Politics/**/*.cs` 전체 support 변경 지점
- `Assets/Scripts/Core/EventBus.cs`

### 4. 포로 처리 수치 변경

새 기획서:

- 석방: 해당 기물의 가치 * 5 만큼 명예 증가.
- 처형: 해당 기물의 가치 * 5 만큼 명예 감소.
- 몸값: 해당 기물의 가치 * 100 만큼 골드 증가.
- 기물 처분 시 플레이어 선택이 상대 플레이어에게 영향을 미치지 않음.

현재 상태 추정:

- `PrisonerPanel`은 고정값:
  - 석방 `+20 명예`
  - 처형 `-20 명예`
  - 몸값 `+200 골드`
- 새 기획서 기준으로 수정 필요.

관련 파일:

- `Assets/Scripts/UI/PrisonerPanel.cs`
- `Assets/Scripts/Chess/Rules/BoardEvaluator.cs`

### 5. 정치형태 효과의 확률/가중치 표현 변경

새 기획서에서 "확률" 표현이 "보드 전체 가중치"로 바뀐 부분이 있음.

성군:

- 아군 기물의 배신 가중치 -20
- 상대 기물의 배신 가중치 +10
- 상대 정치 행동에 의한 명예/지지도 감소율 -50%
- 아군 기물 사망 시 자신의 모든 기물 지지도와 명예가 기물 가치만큼 감소

암군:

- 진입 시 1회:
  - 모든 기물 기본 세금 -30%
  - 보드 전체 수락 가중치 -30
  - 모든 기물 지지도 -10
- 매 턴:
  - 기본 세금 +5%
  - 보드 전체 수락 가중치 +5
  - 모든 기물 지지도 +1
- 20턴 생존 후:
  - 성군 고정
  - 모든 기물 기본 세금 +200%
  - 보드 전체 수락 가중치 +100
  - 매 턴 모든 기물 지지도 +5

폭군:

- 처형 정치행동 소모 없음
- 처형 시 대상이 폰이 아니면 특수 폰 소환
- 처형으로 인한 지지도/명예 감소율 -80%
- 세금 +400%
- 보드 전체 수락 가중치 +400
- 보드 전체 반란 가중치 +30
- 보드 전체 배신 가중치 +30

현재 상태 추정:

- `KingStateEffectApplier`, `KingStateEvaluator`, `RebellionSystem`, `ObedienceSystem`에 구현이 흩어져 있음.
- 세금/처형/성군 일부는 구현됐지만, "보드 전체 가중치" 구조가 없어서 새 사양과 완전히 일치하지 않음.

관련 파일:

- `Assets/Scripts/Core/KingStateEffectApplier.cs`
- `Assets/Scripts/Core/KingStateEvaluator.cs`
- `Assets/Scripts/Core/RebellionSystem.cs`
- `Assets/Scripts/Chess/Rules/ObedienceSystem.cs`
- `Assets/Scripts/Politics/Political/ExecutionAction.cs`

### 6. 사채는 재설계 대상

새 기획서의 사채 기능란이 `다시만들어야함`으로 바뀌었다.

현재 상태 추정:

- `LoanAction`은 후보 선정/상환 대기 구조가 들어가 있음.
- 새 기획서상 확정 사양이 아니므로, 현재 구현은 "구버전/임시 구현"으로 보는 것이 안전.

권장:

- 새 사채 상세 사양을 사용자에게 확인받기 전까지 큰 수정하지 말 것.
- 단, TODO에는 "현재 사채는 재설계 대기"라고 명시.

관련 파일:

- `Assets/Scripts/Politics/Fiscal/LoanAction.cs`

### 7. 정치행동 구현 정확도

현재 구현된 것으로 보이는 정치행동:

- 재정: 특세, 감면, 지원, 징발, 사채, 후방배치, 전방배치
- 군사: 바리케이드, 도로계획, 트레뷰셋, 투석, 전초기지, 신의 힘, 기적, 군법면제
- 정치: 정찰, 제후국, 심문, 매수, 정보, 배신-접촉, 배신-정보, 배신-실책, 배신-파벌, 배신-암살, 암살, 선전, 대민지원, 방문, 벌금, 처형, 선동, 여론조작

주의:

- "버튼/실행 경로"는 대부분 구현된 것으로 보임.
- 하지만 새 기획서의 판정 공식, 가중치 표현, 반란 누적 규칙까지 1:1 정확 구현은 아님.
- 다음 세션에서 "정치행동 정확 구현"을 하려면 목록 추가보다 판정 시스템 정리가 우선.

## 명예/불명예 기물별 행동

새 기획서에 턴 시작 발동 순서가 명시됨.

발동 순서:

1. 폰 → 나이트 → 비숍 → 룩 → 퀸 순서.
2. 퀸사이드 기물 우선, a파일 → h파일 순으로 우선도가 높음.
3. 버프 먼저 체크, 버프 발동 성공 시 디버프 체크 스킵.

현재 상태 추정:

- `HonorPiecePassiveSystem`에 명예 20 이상 패시브 일부 구현됨.
- `DISHONOR_THRESHOLD`는 있으나 불명예 패시브 구현은 부족함.
- 위의 정확한 순서/우선순위 엔진은 아직 없음.

새 기획서 변경/구체화:

- 불명예 쪽 추가 페널티:
  - 지지도가 감소했을 경우, 이전 턴에 감소한 모든 지지도 합 10당 모든 기물 지지도 -1. 소수점 올림.
  - 기물을 잃었을 경우, 잃은 기물 가치만큼 모든 기물 지지도와 명예 감소.

관련 파일:

- `Assets/Scripts/Core/HonorPiecePassiveSystem.cs`
- `Assets/Scripts/Core/EventBus.cs`
- `Assets/Scripts/Core/KingStateEffectApplier.cs`
- `Assets/Scripts/Chess/Pieces/ChessPiece.cs`

## 구현 우선순위 제안

1. 개체별 기본 데이터 구조 정리
   - `PieceLane` 추가
   - 폰 8개 개체 구분
   - 초기 세금/지지도 표 반영
2. 판정 가중치 시스템 정리
   - 보드 전체 수락/배신/반란 가중치
   - 기물별 기타 가중치
   - 이동수락/일반수락/배신/반란 공식 통일
3. 포로 처리 수치 변경
   - 가치 기반 명예/골드
4. 정치형태 효과를 새 가중치 시스템에 재연결
5. 반란 누적 3회 및 지지도 하락 트리거 구현
6. 명예/불명예 패시브 순서화
7. 사채는 새 상세 사양 확인 후 재작성

## 다음 세션 시작 체크리스트

1. `AGENTS.md` 전체 읽기.
2. `git status --short` 확인.
3. `CODEX_TODO_NEXT_SESSION.md` 읽기.
4. Unity MCP 연결 상태 확인.
5. 새 작업 전, 사용자가 "어디부터 할지" 지정하지 않았다면 다음 순서 추천:
   - 먼저 `PieceLane` + 기본 기물 데이터 반영부터.
6. 코드 수정 후 Unity 컴파일 확인.
7. 가능하면 플레이 모드 진입 확인.

## 참고: 절대 바로 다시 하지 말 것

- 보드 회전/확대 `BoardViewController` 작업은 사용자가 "아직 내가 건들일 단계는 아닌 것 같다"며 실행 취소했다고 했다.
- 새 세션에서 사용자가 다시 요청하기 전까지 이 기능은 구현하지 말 것.
