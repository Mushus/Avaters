using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Mushus.CaptureTools
{
    public static class AvatarThumbnailGenerator
    {
        public static void GenerateMarketingImages(AvatarCaptureSettings settings)
        {
            if (settings == null) return;
            string packageName = string.IsNullOrEmpty(settings.PackageName) ? settings.TargetAvatar.name : settings.PackageName;
            
            // "Dev" が含まれる場合は削除して保存先ディレクトリを決定
            string basePackageName = packageName.Replace("Dev", "");
            
            GenerateBoothThumbnails(settings, packageName, basePackageName);
            GenerateExpressionTile(settings, packageName, basePackageName);
        }

        private static void GenerateBoothThumbnails(AvatarCaptureSettings settings, string packageName, string basePackageName)
        {
            string sourcePath = $"Assets/Mushus/{packageName}/Previews/";
            string destPath = $"Products/{basePackageName}/booth/";

            string[] views = { "01_Front", "02_Back", "03_Side", "04_FaceUp" };
            string[] labels = { "FRONT", "BACK", "SIDE", "FACE" };

            string bgColorHex = $"#{ColorUtility.ToHtmlStringRGB(settings.ThumbnailBackgroundColor)}";
            string pythonPath = "python"; // 環境に合わせて調整
            string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), ".agents/scripts/process_thumbnail.py");

            for (int i = 0; i < views.Length; i++) 
            {
                string srcFile = Path.Combine(Application.dataPath, sourcePath.Replace("Assets/", ""), views[i] + ".png");
                if (!File.Exists(srcFile)) continue;

                string destFile = Path.Combine(Directory.GetCurrentDirectory(), destPath, $"thumbnail_{views[i].ToLower()}.png");
                
                // Pythonスクリプトを呼び出し
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = pythonPath;
                process.StartInfo.Arguments = $"\"{scriptPath}\" \"{srcFile}\" \"{destFile}\" \"{bgColorHex}\" \"{labels[i]}\" \"{settings.ThumbnailFontPath}\" {settings.ThumbnailFontSize} {settings.ThumbnailLabelPadding}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[Python Error] {output}");
                }
                else
                {
                    Debug.Log($"[Thumbnail] Generated: {destFile}");
                }
            }
        }

        private static void GenerateExpressionTile(AvatarCaptureSettings settings, string packageName, string basePackageName)
        {
            string iconDirPath = Path.Combine(Application.dataPath, $"Mushus/{basePackageName}/Icons/");
            string destFile = Path.Combine(Directory.GetCurrentDirectory(), $"Products/{basePackageName}/booth/thumbnail_expressions.png");
            
            if (!Directory.Exists(iconDirPath)) return;

            // アイコン一覧を取得 (None.png を先頭に、それ以外を名前順に)
            var files = Directory.GetFiles(iconDirPath, "*.png")
                .Select(f => Path.GetFileName(f))
                .OrderBy(f => f == "None.png" ? 0 : 1)
                .ThenBy(f => f)
                .Select(f => Path.Combine(iconDirPath, f))
                .ToList();

            if (files.Count == 0) return;

            string bgColorHex = $"#{ColorUtility.ToHtmlStringRGB(settings.ThumbnailBackgroundColor)}";
            string pythonPath = "python";
            string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), ".agents/scripts/process_thumbnail.py");
            
            string label = $"全{files.Count}種類";
            string iconFilesArg = string.Join("|", files);

            // Pythonスクリプトを呼び出し (TILEモード)
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = pythonPath;
            process.StartInfo.Arguments = $"\"{scriptPath}\" TILE \"{destFile}\" \"{bgColorHex}\" \"{label}\" \"{settings.ThumbnailFontPath}\" {settings.ThumbnailFontSize} {settings.ThumbnailLabelPadding} \"{iconFilesArg}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.LogError($"[Python Error] {output}");
            }
            else
            {
                Debug.Log($"[Thumbnail] Generated expression tile: {destFile}");
            }
        }
    }
}
