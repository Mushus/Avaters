import sys
import os
import math
from PIL import Image, ImageDraw, ImageFont

def get_font(font_path, font_size):
    try:
        return ImageFont.truetype(font_path, font_size)
    except:
        print(f"Warning: Font not found at {font_path}, using default.")
        return ImageFont.load_default()

def process_single(src_path, dest_path, bg_color_hex, text, font_path, font_size, padding):
    if not os.path.exists(src_path):
        print(f"Error: Source not found: {src_path}")
        return

    img = Image.open(src_path).convert("RGBA")
    bg_color = tuple(int(bg_color_hex.lstrip('#')[i:i+2], 16) for i in (0, 2, 4))
    background = Image.new("RGBA", img.size, bg_color + (255,))
    combined = Image.alpha_composite(background, img)
    
    draw = ImageDraw.Draw(combined)
    font = get_font(font_path, font_size)
    
    bbox = draw.textbbox((0, 0), text, font=font)
    text_w = bbox[2] - bbox[0]
    text_h = bbox[3] - bbox[1]
    
    x = img.width - text_w - padding
    y = img.height - text_h - padding
    
    draw.text((x, y), text, font=font, fill=(255, 255, 255, 255))
    
    os.makedirs(os.path.dirname(dest_path), exist_ok=True)
    combined.save(dest_path, "PNG")
    print(f"Success: {dest_path}")

def process_tile(dest_path, bg_color_hex, text, font_path, font_size, padding, icon_files_str):
    icon_paths = icon_files_str.split('|')
    if not icon_paths:
        return

    icons = [Image.open(p).convert("RGBA") for p in icon_paths if os.path.exists(p)]
    if not icons:
        return

    count = len(icons)
    cell_w, cell_h = icons[0].size
    tile_padding = 10
    
    # 下部のテキストエリア用余白の概算
    footer_h_approx = font_size + padding * 2
    
    # 全体のサイズが正方形に近づくように行列数を計算
    # (cols * cell_w) / (rows * cell_h + footer_h) ≈ 1
    # rows = ceil(count / cols)
    
    best_ratio_diff = float('inf')
    best_cols = math.ceil(math.sqrt(count))
    best_rows = math.ceil(count / best_cols)
    
    for c in range(1, count + 1):
        r = math.ceil(count / c)
        w = c * cell_w + (c + 1) * tile_padding
        h = r * cell_h + (r + 1) * tile_padding + footer_h_approx
        ratio = w / h
        diff = abs(1.0 - ratio)
        if diff < best_ratio_diff:
            best_ratio_diff = diff
            best_cols = c
            best_rows = r
            
    cols = best_cols
    rows = best_rows
    
    # 全体のサイズ計算
    total_w = cols * cell_w + (cols + 1) * tile_padding
    total_h = rows * cell_h + (rows + 1) * tile_padding
    
    # 実際のテキストエリアの高さを計算
    footer_h = font_size + padding * 2
    canvas_h = total_h + footer_h
    
    # 最終的なキャンバスサイズを正方形に設定
    square_size = max(total_w, canvas_h)
    
    bg_color = tuple(int(bg_color_hex.lstrip('#')[i:i+2], 16) for i in (0, 2, 4))
    canvas = Image.new("RGBA", (square_size, square_size), bg_color + (255,))
    
    # 描画オフセットを計算 (中央寄せ)
    offset_x = (square_size - total_w) // 2
    offset_y = (square_size - canvas_h) // 2
    
    # アイコンを配置
    for i, icon in enumerate(icons):
        col = i % cols
        row = i // cols
        x = offset_x + tile_padding + col * (cell_w + tile_padding)
        y = offset_y + tile_padding + row * (cell_h + tile_padding)
        canvas.paste(icon, (x, y), icon)
        
    # テキスト描画
    draw = ImageDraw.Draw(canvas)
    font = get_font(font_path, font_size)
    bbox = draw.textbbox((0, 0), text, font=font)
    text_w = bbox[2] - bbox[0]
    
    # 中央下に配置
    tx = offset_x + (total_w - text_w) // 2
    ty = offset_y + total_h + padding
    draw.text((tx, ty), text, font=font, fill=(255, 255, 255, 255))
    
    os.makedirs(os.path.dirname(dest_path), exist_ok=True)
    canvas.save(dest_path, "PNG")
    print(f"Success (Square Tile): {dest_path} ({square_size}x{square_size})")

if __name__ == "__main__":
    if len(sys.argv) < 8:
        print("Usage:")
        print("  Single: python process_thumbnail.py <src> <dest> <bg_color> <text> <font_path> <font_size> <padding>")
        print("  Tile:   python process_thumbnail.py TILE <dest> <bg_color> <text> <font_path> <font_size> <padding> <icon_paths_sep_by_pipe>")
    else:
        mode = sys.argv[1]
        if mode == "TILE":
            process_tile(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], int(sys.argv[6]), int(sys.argv[7]), sys.argv[8])
        else:
            process_single(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], int(sys.argv[6]), int(sys.argv[7]))
