# Codex Handoff

## Current Goal
- Continue Unity work for `C:\GitHub\MMB_Game`.
- Latest user request: fix actual play-flow issues found by user verification.
- Politics-phase movement blocking, button action application, profile refresh, and value input prompt are complete.

## Important Project Rules
- Read `AGENTS.md` before work.
- Namespace: `MMBGame`.
- Explicit access modifiers required.
- No code comments.
- Keep all script files at or below 300 lines.
- Manager classes should coordinate only; put feature logic in focused files.
- After changes, update `AGENTS.md` component connection/completion sections.
- Do not revert unrelated user changes.

## Unity MCP State
- Unity MCP is connected.
- Active scene verified: `Assets/SampleGame.unity`.
- Script compilation passed with 0 script errors.
- The only remaining console error observed is unrelated to this work:
  - Coplay custom main toolbar element uses an unsupported Unity toolbar method.

## Verification Complete
1. Confirmed Unity MCP connection.
2. Confirmed active scene: `Assets/SampleGame.unity`.
3. Refreshed scripts and requested Unity compilation.
4. Confirmed 0 script compile errors in the Unity console.
5. Entered Play Mode.
6. Verified all 18 political action buttons execute through `CommandActionButton.Execute()` into `PoliticalManager`:
   - `정찰`, `제후국`, `심문`, `매수`, `정보`
   - `배신 - 접촉`, `배신 - 정보`, `배신 - 실책`, `배신 - 파벌`, `배신 - 암살`
   - `암살`, `선전`, `대민지원`, `방문`, `벌금`, `처형`, `선동`, `여론조작`
7. Verified `SpecialTax` hover tooltip creates the expected text:
   - `조건 : 없음`
   - `기능 : 선택된 기물의 턴 당 세금 +100%, 지지도 +5`
8. Verified tooltip box is `8.4 x 1.8`, text font size is `44`, and tooltip hover collider matches the enlarged box.
9. Verified tooltip hides after `CommandActionTooltip.Hide()`.
10. Verified bottom-left `TurnStatusDisplay` updates:
   - initial: `백 정치 턴`
   - after phase end: `백 체스 턴`
   - after next phase end: `흑 정치 턴`
11. Verified politics-phase movement is blocked through `BoardManager.TryMove`.
12. Verified `SpecialTaxButton` is connected and changes selected piece data.
13. Verified selected profile support text refreshes after piece data changes.
14. Verified value-required action flow:
   - click `AidActionButton`
   - input prompt appears
   - submit value
   - prompt hides
   - action applies to selected piece
15. Exited Play Mode.

## Build Verification Attempt
- `dotnet build MMB_Game.slnx` was attempted.
- First attempt failed because sandbox could not write to `C:\Users\hyuns\.dotnet`.
- Second attempt with `DOTNET_CLI_HOME=C:\GitHub\MMB_Game\.dotnet_home` reached MSBuild but failed because Unity-generated project files are missing:
  - `Assembly-CSharp.csproj`
  - `Assembly-CSharp-Editor.csproj`
- Therefore real compile verification must be done through Unity after MCP reconnect.
- A `.dotnet_home` folder was created by that attempt. It is not needed; user denied deletion. Leave it unless user asks cleanup.

## Files Changed For Latest Request
- `Assets/SampleGame.unity`
- `Assets/Scripts/Chess/Board/BoardInteraction.cs`
- `Assets/Scripts/Chess/Board/BoardPieceVisual.cs`
- `Assets/Scripts/UI/CommandActionButton.cs`
- `Assets/Scripts/UI/CommandActionInputPrompt.cs`
- `Assets/Scripts/UI/CommandActionInputPrompt.cs.meta`
- `Assets/Scripts/UI/CommandActionInputRequirements.cs`
- `Assets/Scripts/UI/CommandActionInputRequirements.cs.meta`
- `Assets/Scripts/UI/CommandActionTooltip.cs`
- `Assets/Scripts/UI/CommandActionTooltip.cs.meta`
- `Assets/Scripts/UI/CommandActionTooltipDescriptions.cs`
- `Assets/Scripts/UI/CommandActionTooltipDescriptions.cs.meta`
- `Assets/Scripts/UI/TurnStatusDisplay.cs`
- `Assets/Scripts/UI/TurnStatusDisplay.cs.meta`
- `Assets/Scripts/Politics/Fiscal/TaxActions.cs`
- `Assets/Scripts/Politics/Fiscal/ResourceActions.cs`
- `Assets/Scripts/Politics/Military/MilitaryManager.cs`
- `Assets/Scripts/Politics/Military/RoadPlanAction.cs`
- `Assets/Scripts/Politics/Military/RoadPlanAction.cs.meta`
- `Assets/Scripts/Politics/Political/PoliticalManager.cs`
- `Assets/Scripts/Politics/Political/ReconAction.cs`
- `Assets/Scripts/Politics/Political/PropagandaAction.cs`
- `Assets/Scripts/Politics/Political/AgitationAction.cs`
- `Assets/Scripts/Politics/Political/CivilAidAction.cs`
- `Assets/Scripts/Politics/Political/ManipulationAction.cs`
- `Assets/Scripts/Politics/Political/FeudalStateAction.cs`
- `Assets/Scripts/Politics/Political/BribeAction.cs`
- `Assets/Scripts/Politics/Political/BetrayalActions.cs`
- `Assets/Scripts/Politics/Political/BetrayalActions.cs.meta`
- `Assets/Scripts/Chess/Board/BoardState.cs`
- `Assets/Scripts/Chess/Movement/MoveGenerator.cs`
- `Assets/Scripts/Chess/Movement/SpecialMoves.cs`
- `AGENTS.md`
- `CODEX_HANDOFF.md`

## Latest Implementation Summary
- UI/Button connection:
  - `CommandActionButton` now resolves current turn color from `TurnManager`.
  - `CommandActionButton` now resolves the currently selected board piece through `BoardInteraction`.
  - `CommandActionButton` now supplies a positive fallback input value when a button has no serialized value.
  - `CommandActionButton` now opens `CommandActionInputPrompt` for value-required actions before applying the action.
  - `CommandActionInputPrompt` captures numeric keyboard input, applies on Enter, cancels on Esc, and hides after submit.
  - `CommandActionInputRequirements` lists actions that need numeric input.
  - `BoardInteraction` now blocks movement during non-chess phases while preserving piece selection/profile viewing.
  - `BoardInteraction` refreshes selected move indicators/profile after action and phase events.
  - `BoardManager.TryMove` now rejects movement outside ChessPhase.
  - `CommandActionButton` now shows `CommandActionTooltip` on hover and hides it on hover exit/click.
  - `BoardPieceVisual` keeps a reference to its `ChessPiece`.
  - `BoardInteraction` exposes the currently selected `ChessPiece`.
  - `CommandActionTooltip` creates a larger transparent grey background with smaller white text and includes condition/cost/effect descriptions for command actions.
  - `CommandActionTooltip` keeps hover active over the tooltip box and supports mouse-wheel scrolling for overflow text.
  - `CommandActionTooltipDescriptions` stores the command description text separately so tooltip code stays under 300 lines.
  - `TurnStatusDisplay` anchors to the bottom-left camera viewport and shows current color/phase.
  - `TurnStatusDisplay` subscribes to `EventBus.OnTurnChanged` and `EventBus.OnPhaseChanged`.
  - Added missing political buttons to `Assets/SampleGame.unity`:
    - `방문`
    - `배신 - 접촉`
    - `배신 - 정보`
    - `배신 - 실책`
    - `배신 - 파벌`
    - `배신 - 암살`
- Fiscal:
  - 특세 now doubles tax for one turn and adds support +5.
  - 감면 now exempts tax for one turn and subtracts support -5.
  - 지원/징발 now scale support by `inputValue / target.taxPerTurn`.
- Military:
  - Added `RoadPlanAction`.
  - `MilitaryManager` now registers `RoadPlanAction`.
  - `BoardState` now stores one active road endpoint pair.
  - `MoveGenerator` adds road movement between road endpoints.
  - `SpecialMoves` clears the road after it is used.
- Political:
  - 정찰 now publishes the opponent’s last 3 actions.
  - 선전 changed to support +`currentSupport / 20`.
  - 선동 changed to support -`(100 - currentSupport) / 40`.
  - 대민지원 changed to allied-wide support +`ceil(value / (sumBaseTax / 2))`.
  - 여론조작 changed to allied-wide support -`ceil(value / (sumBaseTax / 3))`.
  - 제후국 now spends `value * 3`, applies 3 rebellion checks, and adds `value / taxPerTurn` rebellion weight.
  - 매수 now spends `value * 3` and runs 3 betrayal checks.
  - Added 배신 하위 행동:
    - `배신 - 접촉`
    - `배신 - 정보`
    - `배신 - 실책`
    - `배신 - 파벌`
    - `배신 - 암살`

## Notes About Action API Limitations
- Current `CommandActionButton`/manager APIs support one selected piece, one value, and one target coordinate pair.
- `RoadPlanAction` uses selected piece as endpoint A and `targetFile/targetRank` as endpoint B.
- `배신 - 암살` currently uses the betrayed selected enemy as assassin and automatically chooses a same-color non-betrayed victim that the assassin can reach. There is no second explicit target in the existing API yet.
- These are functional implementations within the current architecture, but UI may need follow-up if the user wants two-click target selection for road/victim selection.

## Existing Pre-Latest Work Context
- Board coordinate mapping was changed so logical tiles use `Tile_a1` through `Tile_h8`.
- White is at the bottom/player side in the current visual orientation.
- User-specified corner remap was implemented:
  - old `h1 -> a1`
  - old `h8 -> a8`
  - old `a1 -> h1`
  - old `a8 -> h8`
- Initial placement was rotated:
  - White back rank on file `a`, white pawns on file `b`.
  - Black back rank on file `h`, black pawns on file `g`.
- Piece click interaction was added through `BoardInteraction`.
- Legal move indicators were added through `BoardMoveIndicators`:
  - empty legal moves: translucent white circles
  - captures: translucent grey circles
- Piece profile display was added through `BoardPieceProfileDisplay`.

## AGENTS.md Update Status
- `AGENTS.md` was updated for:
  - `BoardState` road state relationship.
  - `MilitaryAction` references including `MovePattern`.
  - `PoliticalAction` references including `MoveGenerator` and `SpecialMoves`.
  - Completion section for 2026-05-06 political-action document implementation.

## Suggested Next Response After Verification
- If Unity console has no compile errors, tell the user:
  - Implementation is complete.
  - MCP compile check passed.
  - Mention any runtime smoke-test result.
- If errors appear, fix up to two rounds, then report exact errors and attempted fixes.
