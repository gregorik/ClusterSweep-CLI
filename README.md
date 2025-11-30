# ClusterSweep (Core CLI)

A simplified, open-source command-line interface for the ClusterSweep pixel art cleanup algorithm.

**For the Full GUI Version with Real-time Preview, Drag-and-Drop, and Visual Tools, visit:**
👉 **[Get ClusterSweep Pro on Itch.io](https://gregorigin.itch.io/clustersweep)**

## What is this?
This tool processes pixel art to remove:
1. **Orphans:** Stray 1x1 pixels (noise).
2. **Gradients:** Unwanted anti-aliasing color drift.

## Usage
`ClusterSweep-CLI.exe <input.png> [flags]`

### Flags:
- `--clean <number>` : Runs the Despeckle algorithm X times.
- `--snap <0-100>`   : Snaps colors to reducing gradients (Euclidean distance).
- `-o <filename>`    : Output file path.

### Example
```bash
ClusterSweep-CLI.exe character.png -o clean.png --clean 3 --snap 15
