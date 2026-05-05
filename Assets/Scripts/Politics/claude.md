# Politics — claude.md

> 세션 시작 시 루트 CLAUDE.md → 이 파일 순서로 읽을 것

---

## 이 폴더의 역할
정치행동 페이즈 전담. 재정/군사/정치 3개 카테고리로 분리.
체스 로직에 직접 접근하지 않고 BoardManager, PlayerState 경유.

## 폴더 구조 및 파일 목록

### 루트
| 파일 | 역할 |
|------|------|
| PoliticsManager.cs | 정치행동 페이즈 관리, 턴 시작 시 goldPerTurn 지급, 기물 세금 배율 초기화 |

### Fiscal/ (재정)
| 파일 | 역할 |
|------|------|
| FiscalAction.cs | 추상 기본 클래스 |
| SpecialTax.cs | 특세 — 기물 세금 1턴 2배 |
| TaxExemption.cs | 감면 — 기물 세금 1턴 면제 |
| Support.cs | 지원 — 골드→지지도 전환 |
| Requisition.cs | 징발 — 지지도→골드 전환 |
| PrivateLoan.cs | 사채 — 기물 대출/이자 상환 |
| RearDeployment.cs | 후방배치 — 기물 체스판 밖 이동 |
| FrontDeployment.cs | 전방배치 — 후방 기물 복귀 |

### Military/ (군사)
| 파일 | 역할 |
|------|------|
| MilitaryAction.cs | 추상 기본 클래스 |
| DelayedEffect.cs | 예약 효과 처리 |
| BarricadeAction.cs | 바리케이드 설치 |
| TrebuchetAction.cs | 트레뷰셋 설치 |
| SiegeAction.cs | 투석 |
| OutpostAction.cs | 전초기지 (나이트 강화) |
| DivinePowerAction.cs | 신의 힘 (룩 강화) |
| MiracleAction.cs | 기적 (비숍 강화) |
| MartialLawExemption.cs | 군법면제 (폰 강화) |

### Political/ (정치)
| 파일 | 역할 |
|------|------|
| PoliticalAction.cs | 추상 기본 클래스 |
| Propaganda.cs | 선전 |
| CivilAid.cs | 대민지원 |
| Reconnaissance.cs | 정찰 |
| Visit.cs | 방문 |
| Fine.cs | 벌금 |
| Execution.cs | 처형 |
| Incitement.cs | 선동 |
| PublicManipulation.cs | 여론조작 |
| Bribery.cs | 매수 |
| Intelligence.cs | 정보 |
| Assassination.cs | 암살 (기획 중) |
| Contact.cs | 접촉 (기획 중) |

## 코드 스타일
- 네임스페이스: `MMBGame`
- Manager는 조율만 — 실제 효과는 Action 서브클래스에서 처리
- 300줄 초과 시 즉시 분리

---

## PlayerState 필드
```csharp
int gold
int goldPerTurn
int honor
void AddHonor(int amount)
Action OnHonorChanged
```

---

## 작업 완료 목록

### 페이즈 3 (2026-04-05)
- PlayerState, FiscalAction 추상 클래스
- 특세/감면, 지원/징발, 사채, 후방배치/전방배치 구현
- PoliticsManager: 턴 변경 시 세금 배율 초기화, goldPerTurn 자동 지급

### 페이즈 4 (2026-04-05)
- MilitaryAction, DelayedEffect
- 바리케이드/트레뷰셋/투석/전초기지/신의힘/기적/군법면제 구현
- MilitaryManager 추가
- EventBus에 예약 제거 이벤트 추가

### 페이즈 5 (2026-04-05)
- PoliticalAction 추상 클래스
- 정찰/매수/정보/암살/선전/대민지원/방문/벌금/처형/선동/여론조작 구현
- PoliticalManager 추가
- PoliticsManager/MilitaryManager 성공 행동 이벤트 발행 추가

---

## 해야 할 작업

- [ ] 암살: 행마법 겹치는 적 기물 제거 로직 구현
- [ ] 접촉: 기획 완료 후 구현
- [ ] 페이즈 6: 기물 성격이 정치행동에 반응하는 트리거 연결
  - 예) 폰 자치권 요구 → 수락/거부 처리
  - 예) 퀸사이드 룩 군자금 요청 → 수락/거부 처리
- [ ] 페이즈 7: 정치행동 UI 연결 (PoliticsUI.cs)
