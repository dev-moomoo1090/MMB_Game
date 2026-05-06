# Codex Handoff

## Current Goal
- Continue Unity work for `C:\GitHub\MMB_Game`.
- User wants two next changes:
  - Check whether piece click/move interaction exists; implement if missing.
  - Change initial chess piece layout from left/right orientation to top/bottom orientation.
- Use Unity MCP after restart to verify in the actual scene and Game View.

## Important Project Rules
- Read `AGENTS.md` before work.
- Namespace: `MMBGame`.
- Explicit access modifiers required.
- No code comments.
- Keep all script files at or below 300 lines.
- Manager classes should coordinate only; put feature logic in focused files.
- Do not modify files outside the requested scope.
- After changes, update `AGENTS.md` component connection/completion sections.

## Current Scene Setup
- Scene: `Assets/SampleGame.unity`.
- Scene object exists and was saved through Unity MCP:
  - `Managers/PieceSetupManager`
  - Component: `BoardPieceSetupManager`
- `BoardManager` references `PieceSetupManager` via serialized `pieceSetupManager`.
- Edit piece setup in Unity Inspector at:
  - `Managers > PieceSetupManager > BoardPieceSetupManager > pieceDefinitions`

## Current Script Structure
- `Assets/Scripts/Chess/Board/BoardManager.cs`
  - Coordinates board state, move attempts, obstacle placement, rear/front deploy, result checks.
  - Finds or creates `BoardPieceSetupManager` if reference is missing.
  - Calls setup manager for piece setup, promotion setup, and visual refresh.
- `Assets/Scripts/Chess/Board/BoardPieceSetupManager.cs`
  - Owns `pieceDefinitions`.
  - Applies initial support, tax, base move patterns, and side resolution to pieces.
  - Syncs visuals through `BoardPieceVisuals`.
  - Has context menu `Fill Default Piece Definitions`.
  - Has context menu `Assign Missing Prefab References`.
  - Auto-fills empty definitions and assigns missing prefab references in editor using delayed calls.
  - Missing prefab references fall back to same-color pawn prefabs.
- `Assets/Scripts/Chess/Board/BoardPieceVisuals.cs`
  - Spawns prefabs or sprites at board tile positions.
  - Prefab is preferred over sprite.
  - Runtime visual scale is currently `0.55`.
  - Piece model bottom-center is aligned to tile center, so pieces stand on the tile instead of centering their body on the tile.
  - Uses selected prefab/sprite when a piece visual is selected.
- `Assets/Scripts/Chess/Board/BoardPieceVisual.cs`
  - Handles clickable runtime piece visual and selected visual state only.
  - It does not yet appear to implement actual move destination selection or move execution.
- `Assets/Scripts/UI/CommandActionButton.cs`
  - Handles command action execution and pointer press/hover state.
- `Assets/Scripts/UI/CommandActionButtonVisualFeedback.cs`
  - Handles text-sized collider, text outline hover, and press color feedback.

## Work Completed In Latest Session
- Unity MCP became available after restart and was used successfully.
- Created `Managers/PieceSetupManager` in the loaded scene when it was missing from the editor instance.
- Added `BoardPieceSetupManager` component and connected `BoardManager.pieceSetupManager`.
- Saved `Assets/SampleGame.unity`.
- Added automatic default piece definitions when `pieceDefinitions` is empty.
- Added editor-side missing prefab assignment.
- Added same-color pawn prefab fallback for missing piece prefabs.
- Manually filled remaining Black queenside null prefab slots in Unity MCP:
  - Black queenside rook fallback: `Black_Pawn_Queenside`
  - Black queenside knight fallback: `Black_Pawn_Queenside`
  - Black queenside bishop fallback: `Black_Bishop_Kingside` or fallback depending current scene serialization; recheck after restart.
- Adjusted piece visual scale from `1.0` to `0.55`.
- Adjusted placement so the visual model's bottom-center aligns with the tile center.
- Updated `AGENTS.md` to mention the prefab/position/scale correction.

## Visual Verification
- Game View screenshots were captured:
  - `Assets/Screenshots/codex_game_view_piece_size_before.png`
  - `Assets/Screenshots/codex_game_view_piece_size_after_055.png`
  - `Assets/Screenshots/codex_game_view_piece_offset_y0.png`
  - `Assets/Screenshots/codex_game_view_piece_tile_bounds_center.png`
  - `Assets/Screenshots/codex_game_view_piece_feet_center.png`
- Best current screenshot:
  - `Assets/Screenshots/codex_game_view_piece_feet_center.png`
- User observed pieces were not centered on board tiles.
- Fix applied: model bottom-center now aligns to tile center.
- Recheck visually after Unity/MCP restart.

## Prefab Naming / Typo State
- User requested not to tolerate typo in code.
- Prefab filenames were renamed from `Kingnside` to `Kingside`.
- Internal prefab `m_Name` values were also changed.
- Search previously verified no old typo matches in actual `Assets` project content, except explanatory text in this handoff may mention the old typo.

## Current Git State Notes
- One commit already exists:
  - `163a0f1 Add Unity project and board piece setup`
- Push failed earlier because GitHub remote returned repository not found:
  - `https://github.com/dev-moomoo1090/MMB_Game.git`
- Current uncommitted changes include, at minimum:
  - `AGENTS.md`
  - `Assets/SampleGame.unity`
  - `Assets/Scripts/Chess/Board/BoardManager.cs`
  - `Assets/Scripts/Chess/Board/BoardPieceSetupManager.cs`
  - `Assets/Scripts/Chess/Board/BoardPieceVisuals.cs`
  - `Assets/Scripts/UI/CommandActionButton.cs`
  - `Assets/Scripts/UI/CommandActionButtonVisualFeedback.cs`
  - Black prefab renames from old typo spelling to `Kingside`
  - Screenshot files under `Assets/Screenshots/`
  - `CODEX_HANDOFF.md`
- Do not stage/commit unless user asks.

## Verification Already Done
- Unity compile check after latest script edits: no compile errors.
- All `Assets/Scripts/**/*.cs` files were checked and were at or below 300 lines.
- Play mode was run multiple times.
- Current expected unrelated console error:
  - Unity/Coplay toolbar error about unsupported custom elements in Unity main toolbar.
  - This appears unrelated to the game scripts.

## Current Unity MCP State
- At the end of the session, Unity MCP became inaccessible:
  - Request failed to `http://127.0.0.1:8080/mcp`.
- User plans to restart Unity/MCP.
- After restart, first check MCP with:
  - `mcpforunity://editor/state`
  - `read_console`
  - `find_gameobjects` for `PieceSetupManager` and `BoardManager`

## Next Recommended Steps After Restart
1. Confirm Unity MCP is connected.
2. Confirm active scene is `Assets/SampleGame.unity`.
3. Confirm `Managers/PieceSetupManager` exists.
4. Confirm `BoardManager.pieceSetupManager` is not null.
5. Enter Play Mode and capture Game View.
6. Recheck piece foot alignment on board tiles.
7. Investigate piece click/move flow:
   - `BoardPieceVisual` currently handles selection visuals.
   - Determine whether any tile click or move-selection component exists.
   - If missing, add a focused board interaction component rather than bloating `BoardManager`.
8. Change initial layout from left/right orientation to top/bottom orientation:
   - Inspect `BoardManager.SetupInitialPosition()`.
   - Current setup likely uses files/ranks in a way that appears side-by-side in the isometric board.
   - Adjust initial coordinates so opposing armies occupy top and bottom sides in Game View.
   - Verify with Unity MCP screenshot.
