# AI Art Source Record

## Source

- Generator/model: Seedance 2.0
- Generation date: 2026-09-19
- Prompt source: `Tools/AIAssets/foundry-prompts.jsonl`
- Original files: this directory
- Processing script: `Tools/process_ai_art.py`
- Processed output: `Assets/Sokoban/Art/Themes/Foundry/`

## Files

- `floor-tile.png`
- `wall-tile.png`
- `matter-block.png`
- `player-unit.png`
- `goal-socket.png`
- `portal-emitter.png`
- `exit-gate.png`

## Processing Performed

1. Detect the magenta background.
2. Keep the central connected subject.
3. Remove background and watermark regions.
4. Trim transparent padding.
5. Center on a square canvas.
6. Resize to 256x256 RGBA.

The original generated files are retained here without modification.

## Rights And Review

The project records the generating model and prompt provenance. The user must
verify that the Seedance account and model terms permit the intended portfolio
or commercial use. If a commercial release is planned, keep the provider
terms, generation records, and any account entitlement evidence with the
project.
