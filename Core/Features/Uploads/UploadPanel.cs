using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Chat;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.Utilities.FileBrowser;
using static nativefiledialog;

namespace ChatPlus.Core.Features.Uploads
{
    public class UploadPanel : BasePanel<Upload>
    {
        protected override int GridColumns => 4;
        protected override int GridCellWidth => 60;
        protected override int GridCellHeight => 60;
        protected override int GridCellPadding => 6;
        protected override BaseElement<Upload> BuildElement(Upload data) => new UploadElement(data);
        protected override IEnumerable<Upload> GetSource() => UploadManager.Uploads;
        protected override string GetDescription(Upload data) => data.FileName;
        protected override string GetTag(Upload data) => data.Tag;

        public override void InsertSelectedTag()
        {
            if (items.Count == 0)
            {
                return;
            }

            if (currentIndex < 0)
            {
                return;
            }

            string insert = GetTag(items[currentIndex].Data);
            if (string.IsNullOrEmpty(insert))
            {
                return;
            }

            string text = Main.chatText ?? string.Empty;
            int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

            // 1) Bare '#' mode outside tags: replace "#query" with the tag
            int hash = -1;
            if (caret > 0)
            {
                int start = Math.Min(caret - 1, Math.Max(0, text.Length - 1));
                hash = text.LastIndexOf('#', start);
            }

            bool hashMode = false;
            if (hash >= 0)
            {
                int lb = text.LastIndexOf('[', hash);
                int rb = text.LastIndexOf(']', hash);
                hashMode = lb <= rb; // outside any [...] tag
            }

            if (hashMode)
            {
                int start = hash;
                int stop = FindStop(text, start + 1);
                if (stop < 0 || stop > caret)
                {
                    stop = caret;
                }

                string before = text.Substring(0, start);
                string after = text.Substring(stop);
                Main.chatText = before + insert + after;
                HandleChatSystem.SetCaretPos(before.Length + insert.Length);
                return;
            }

            // 2) Open bracketed [u... (no closing ] yet): replace the open fragment
            int uStart = text.LastIndexOf("[u", StringComparison.OrdinalIgnoreCase);
            if (uStart >= 0 && caret >= uStart)
            {
                int closing = text.IndexOf(']', uStart + 2);
                bool open = closing == -1;
                if (open)
                {
                    string before = text.Substring(0, uStart);
                    string after = text.Substring(caret);
                    Main.chatText = before + insert + after;
                    HandleChatSystem.SetCaretPos(before.Length + insert.Length);
                    return;
                }
            }

            // 3) Fallback: append at end
            Main.chatText += insert;
            HandleChatSystem.SetCaretPos(Main.chatText.Length);

            static int FindStop(string s, int start)
            {
                if (start >= s.Length)
                {
                    return -1;
                }

                char[] stops = [' ', '\t', '\n', '\r', ']'];
                return s.IndexOfAny(stops, start);
            }
        }
        public void UploadImage()
        {
            string fullFilePath = OpenFileDialog();
            if (string.IsNullOrEmpty(fullFilePath))
                return;

            try
            {
                string fileName = Path.GetFileName(fullFilePath);
                string key = Path.GetFileNameWithoutExtension(fileName);

                // 1) Persist a copy into the mod's uploads folder FIRST
                string folder = Path.Combine(Main.SavePath, "ChatPlus", "Uploads");
                Directory.CreateDirectory(folder);
                string dest = Path.Combine(folder, fileName);

                // If user picked a file inside the same folder with same name, this is harmless
                File.Copy(fullFilePath, dest, true);

                // 2) Load texture from the DESTINATION (ensures file exists before we read & register)
                Texture2D texture;
                using (var fs = File.OpenRead(dest))
                    texture = Texture2D.FromStream(Main.instance.GraphicsDevice, fs);

                if (texture == null)
                {
                    Main.NewText("Failed to load image.", Color.Red);
                    return;
                }

                // 3) Register tag & add/replace in memory
                UploadTagHandler.Register(key, texture);
                string tag = UploadTagHandler.GenerateTag(key);

                UploadManager.AddNewUpload(
                    new Upload(
                        Tag: tag,
                        FileName: fileName,
                        FullFilePath: dest,
                        Texture: texture
                    )
                );

                // 4) Refresh UI
                if (ConnectedPanel is UploadPanel up)
                    up.PopulatePanel();

                Log.Info($"Added {fileName} as {tag}");
                //Main.NewText($"Added {fileName} as {tag}", Color.LightGreen);
            }
            catch (Exception ex)
            {
                Main.NewText("Upload failed: " + ex.Message, Color.Red);
                Log.Error("Upload failed: " + ex);
            }
        }

        private static string OpenFileDialog()
        {
            var extensions = new ExtensionFilter
            {
                Name = "Images",
                Extensions = ["png", "jpg", "jpeg", "gif"]
            };

            // Concatenate extensions for NFD: "png,jpg,jpeg"
            string extensionStr = string.Join(',', extensions.Extensions);

            // Initial directory � use the user's Pictures folder or leave null for default
            //string startDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            nfdresult_t result = NFD_OpenDialog(
                extensionStr,              // filter list
                null,                  // initial directory
                out string outPath);

            if (result == nfdresult_t.NFD_CANCEL)
            {
                // User cancelled the dialog
                Log.Info("File dialog was cancelled by the user.");
                return null;
            }
            if (string.IsNullOrEmpty(outPath))
            {
                // No file was selected
                Log.Error("No file selected.");
                return null;
            }
            if (!File.Exists(outPath))
            {
                // The selected file does not exist
                Log.Error($"File does not exist: {outPath}");
                return null;
            }
            if (result != nfdresult_t.NFD_OKAY)
            {
                // An error occurred
                Log.Error($"Failed to open file dialog: {NFD_GetError()}");
                return null;
            }
            // Log.Info($"Selected file: {outPath}");
            return outPath;
        }

        public void OpenUploadsFolder()
        {
            try
            {
                string folder = Path.Combine(Main.SavePath, "ChatPlus", "Uploads");
                Process.Start(new ProcessStartInfo($@"{folder}")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Main.NewText("Error opening folder: " + ex.Message, Color.Red);
                Log.Error("Error opening client log: " + ex.Message);
            }
        }
    }
}
