import sharp from 'sharp';
import fs from 'fs/promises';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * 指定したディレクトリの画像ファイルをタイル状に並べてPNGファイルとして保存する
 * @param {string} inputDir - 入力画像のディレクトリパス
 * @param {string} outputDir - 出力先ディレクトリパス
 */
async function tileImages(inputDir, outputDir) {
  try {
    // 出力ディレクトリが存在しない場合は作成
    await fs.mkdir(outputDir, { recursive: true });

    // 入力ディレクトリから画像ファイルを取得
    const files = await fs.readdir(inputDir);
    const imageFiles = files.filter(file =>
      /\.(png|jpg|jpeg|webp)$/i.test(file) && !file.endsWith('.meta')
    );

    if (imageFiles.length === 0) {
      console.log('画像ファイルが見つかりませんでした。');
      return;
    }

    console.log(`${imageFiles.length}個の画像ファイルを見つけました。`);

    // 各画像のメタデータを取得
    const imageInfos = await Promise.all(
      imageFiles.map(async (file) => {
        const filePath = path.join(inputDir, file);
        const metadata = await sharp(filePath).metadata();
        return {
          path: filePath,
          name: file,
          width: metadata.width,
          height: metadata.height,
        };
      })
    );

    // タイル配置の計算（正方形グリッド）
    const gridSize = Math.ceil(Math.sqrt(imageFiles.length));
    const columns = gridSize;
    const rows = gridSize;

    const maxWidth = Math.max(...imageInfos.map(img => img.width));
    const maxHeight = Math.max(...imageInfos.map(img => img.height));

    const canvasWidth = maxWidth * columns;
    const canvasHeight = maxHeight * rows;

    console.log(`タイルサイズ: ${columns}列 x ${rows}行（正方形グリッド）`);
    console.log(`出力画像サイズ: ${canvasWidth} x ${canvasHeight}`);

    // 透明なキャンバスを作成
    const canvas = sharp({
      create: {
        width: canvasWidth,
        height: canvasHeight,
        channels: 4,
        background: { r: 0, g: 0, b: 0, alpha: 0 }
      }
    });

    // 各画像を配置するための composite 配列を作成
    const compositeOps = await Promise.all(
      imageInfos.map(async (img, index) => {
        const col = index % columns;
        const row = Math.floor(index / columns);
        const x = col * maxWidth;
        const y = row * maxHeight;

        // 画像を中央揃えにするためのオフセット計算
        const offsetX = Math.floor((maxWidth - img.width) / 2);
        const offsetY = Math.floor((maxHeight - img.height) / 2);

        const buffer = await fs.readFile(img.path);

        return {
          input: buffer,
          left: x + offsetX,
          top: y + offsetY,
        };
      })
    );

    // 画像を合成して保存
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
    const outputPath = path.join(outputDir, `tiled-${timestamp}.png`);

    await canvas
      .composite(compositeOps)
      .png()
      .toFile(outputPath);

    console.log(`\n✓ タイル画像を保存しました: ${outputPath}`);
    console.log(`  - 画像数: ${imageFiles.length}`);
    console.log(`  - レイアウト: ${columns}列 x ${rows}行`);
    console.log(`  - サイズ: ${canvasWidth} x ${canvasHeight}`);

  } catch (error) {
    console.error('エラーが発生しました:', error);
    throw error;
  }
}

// コマンドライン引数の処理
const args = process.argv.slice(2);

if (args.length < 1) {
  console.log('使用方法: npm run tile <入力ディレクトリ> [出力ディレクトリ]');
  console.log('例: npm run tile "./avatars/Assets/Bat/Textures/FaceEmote" "./output"');
  console.log('※ 画像は自動的に正方形グリッド（行と列が同じ数）に配置されます');
  process.exit(1);
}

const inputDir = path.resolve(args[0]);
const outputDir = args[1] ? path.resolve(args[1]) : path.resolve('./output');

console.log('画像タイル処理を開始します...');
console.log(`入力: ${inputDir}`);
console.log(`出力: ${outputDir}\n`);

tileImages(inputDir, outputDir);
