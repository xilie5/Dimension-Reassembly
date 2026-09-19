from __future__ import annotations

import argparse
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image


SOURCE_DIR = Path(r"E:\UnityProjects\Box\Assets\Sokoban\Art\AIArt")
OUTPUT_DIR = SOURCE_DIR / "Processed"


def estimate_background(image: np.ndarray) -> np.ndarray:
    border = np.concatenate(
        [
            image[:32, :, :].reshape(-1, 3),
            image[-32:, :, :].reshape(-1, 3),
            image[:, :32, :].reshape(-1, 3),
            image[:, -32:, :].reshape(-1, 3),
        ]
    )
    return np.median(border, axis=0)


def build_foreground_mask(image: np.ndarray, background: np.ndarray) -> np.ndarray:
    distance = np.linalg.norm(image.astype(np.float32) - background, axis=2)
    return distance > 72.0


def central_component(
    mask: np.ndarray,
    scale: int = 4,
) -> tuple[tuple[int, int, int, int], np.ndarray]:
    height, width = mask.shape
    reduced_h = height // scale
    reduced_w = width // scale
    trimmed = mask[: reduced_h * scale, : reduced_w * scale]
    reduced = trimmed.reshape(reduced_h, scale, reduced_w, scale).max(axis=(1, 3))

    visited = np.zeros_like(reduced, dtype=bool)
    start = (reduced_h // 2, reduced_w // 2)
    if not reduced[start]:
        ys, xs = np.where(reduced)
        if len(xs) == 0:
            raise RuntimeError("No foreground pixels found.")
        distances = (ys - reduced_h // 2) ** 2 + (xs - reduced_w // 2) ** 2
        index = int(np.argmin(distances))
        start = (int(ys[index]), int(xs[index]))

    queue = deque([start])
    visited[start] = True
    min_x = max_x = start[1]
    min_y = max_y = start[0]
    while queue:
        y, x = queue.popleft()
        min_x = min(min_x, x)
        max_x = max(max_x, x)
        min_y = min(min_y, y)
        max_y = max(max_y, y)
        for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
            ny, nx = y + dy, x + dx
            if 0 <= ny < reduced_h and 0 <= nx < reduced_w and reduced[ny, nx] and not visited[ny, nx]:
                visited[ny, nx] = True
                queue.append((ny, nx))

    padding = scale * 3
    bbox = (
        max(0, min_x * scale - padding),
        max(0, min_y * scale - padding),
        min(width, (max_x + 1) * scale + padding),
        min(height, (max_y + 1) * scale + padding),
    )
    ys, xs = np.indices((height, width))
    component = visited[np.minimum(ys // scale, reduced_h - 1), np.minimum(xs // scale, reduced_w - 1)]
    return bbox, component


def process_image(source_path: Path) -> Image.Image:
    image = Image.open(source_path).convert("RGB")
    rgb = np.asarray(image)
    background = estimate_background(rgb)
    mask = build_foreground_mask(rgb, background)
    bbox, component = central_component(mask)
    cropped = rgb[bbox[1] : bbox[3], bbox[0] : bbox[2]]
    cropped_component = component[bbox[1] : bbox[3], bbox[0] : bbox[2]]

    crop_background = estimate_background(cropped)
    crop_distance = np.linalg.norm(cropped.astype(np.float32) - crop_background, axis=2)
    alpha = np.clip((crop_distance - 58.0) / 64.0, 0.0, 1.0)
    alpha[alpha < 0.08] = 0.0
    alpha *= cropped_component.astype(np.float32)

    rgba = np.dstack([cropped, (alpha * 255.0).astype(np.uint8)])
    alpha_mask = rgba[:, :, 3] > 12
    ys, xs = np.where(alpha_mask)
    if len(xs) == 0:
        raise RuntimeError(f"No foreground found after processing {source_path.name}")

    x0, x1 = int(xs.min()), int(xs.max()) + 1
    y0, y1 = int(ys.min()), int(ys.max()) + 1
    subject = rgba[y0:y1, x0:x1]

    side = max(subject.shape[0], subject.shape[1])
    padding = max(8, int(side * 0.08))
    canvas_side = side + padding * 2
    canvas = np.zeros((canvas_side, canvas_side, 4), dtype=np.uint8)
    offset_x = (canvas_side - subject.shape[1]) // 2
    offset_y = (canvas_side - subject.shape[0]) // 2
    canvas[offset_y : offset_y + subject.shape[0], offset_x : offset_x + subject.shape[1]] = subject

    result = Image.fromarray(canvas, "RGBA")
    return result.resize((256, 256), Image.Resampling.LANCZOS)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, default=SOURCE_DIR)
    parser.add_argument("--output", type=Path, default=OUTPUT_DIR)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)
    for source_path in sorted(args.source.glob("*.png")):
        if source_path.parent == args.output:
            continue
        result = process_image(source_path)
        destination = args.output / source_path.name
        result.save(destination, "PNG")
        print(f"{source_path.name}: {result.size} -> {destination}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
