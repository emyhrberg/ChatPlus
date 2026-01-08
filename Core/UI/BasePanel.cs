using System;
using System.Collections.Generic;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Chat;
using ChatPlus.Core.Chat.ChatButtons.Shared;
using ChatPlus.Core.Features.Colors;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.Items;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Uploads;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static ChatPlus.Common.Configs.Config;

namespace ChatPlus.Core.UI;

public abstract class BasePanel<TData> : DraggablePanel
{
    // Viewmode
    public virtual Viewmode CurrentViewMode
    {
        get
        {
            return ChatButtonLayout.GetViewmodeFor(GetType());
        }
    }
    private Viewmode appliedViewMode;
    public bool IsGridModeEnabled => CurrentViewMode == Viewmode.Grid;
    protected CustomGrid grid; // only used in grid mode

    // Grid settings
    protected virtual int GridColumns => 8;
    protected virtual int GridCellWidth => 30;
    protected virtual int GridCellHeight => 30;
    protected virtual int GridCellPadding => 4;

    // Elements
    public UIScrollbar scrollbar;
    protected UIList list;
    public List<BaseElement<TData>> items = [];

    // Force populate
    protected abstract IEnumerable<TData> GetSource(); // The source of data to populate the panel with
    protected abstract BaseElement<TData> BuildElement(TData data); // The method to create a new element from the data
    protected abstract string GetDescription(TData data);
    protected abstract string GetTag(TData data);
    public static bool IsHoveringAnyBasePanel;

    public bool TryGetSelected(out TData data)
    {
        if (currentIndex >= 0 && currentIndex < items.Count && items[currentIndex] != null)
        {
            data = items[currentIndex].Data;
            return true;
        }
        data = default;
        return false;
    }

    // Navigation
    public int CurrentIndex => currentIndex;
    protected int currentIndex = 0; // first item
    private float _lastPanelHeightPx = -1f;

    // Holding keys
    private double repeatTimer;
    private Keys heldKey = Keys.None;
    protected bool JustPressed(Keys key) => Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);

    public BasePanel()
    {
        Width.Set(300, 0);

        VAlign = 1f;
        Top.Set(-64, 0);
        if (Main.screenWidth <= 0)
            Left.Set(300, 0);
        else
            Left.Set(Main.screenWidth - 522, 0);

        OverflowHidden = true;
        BackgroundColor = new Color(33, 43, 79) * 1.0f;
        SetPadding(0);

        list = new UIList
        {
            ListPadding = 0f,
            Width = { Pixels = -20f, Percent = 1f },
            Top = { Pixels = 3f },
            Height = { Pixels = -14f, Percent = 1f },
            Left = { Pixels = 3f },
            ManualSortMethod = _ => { },
        };

        grid = new CustomGrid
        {
            ListPadding = GridCellPadding,
            Width = { Pixels = -20f, Percent = 1f },
            Top = { Pixels = 3f },
            Left = { Pixels = 3f },
            HAlign = 0f,
            Height = { Pixels = -14f, Percent = 1f },
            FixedColumns = GridColumns,
            CellWidth = GridCellWidth,
            CellHeight = GridCellHeight
        };

        scrollbar = new UIScrollbar
        {
            HAlign = 1f,
            Height = { Pixels = -14f, Percent = 1f },
            Top = { Pixels = 7f },
            Width = { Pixels = 20f },
        };

        list.SetScrollbar(scrollbar);
        grid.SetScrollbar(scrollbar);

        Append(list);
        Append(scrollbar);
    }

    private bool positionReady;

    public override void OnActivate()
    {
        base.OnActivate();

        // If a chat button opened us, snap now BEFORE any first draw.
        if (DraggablePanel.TryConsumeNextSnap(out var anchorPos, out var anchorSize))
        {
            SnapRightAlignedTo(anchorPos, anchorSize);
        }
        else
        {
            // Fallback to your default placement computed in the ctor.
            Recalculate();
        }

        positionReady = true;

        appliedViewMode = CurrentViewMode;
        PopulatePanel();

        list.ViewPosition = 0f;
        if (grid._scrollbar != null)
        {
            grid._scrollbar.ViewPosition = 0f;
        }

        currentIndex = 0;
        if (items.Count > 0)
        {
            SetSelectedIndex(0);
        }

        Recalculate();
        Main.oldKeyState = Main.keyState;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!positionReady)
            return;

        base.Draw(spriteBatch);
    }


    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsMouseHovering)
            {
                SetSelectedIndex(i);
                return;
            }
        }
    }

    public void ClearPanel()
    {
        items.Clear();
        list.Clear();
        grid.Clear();
    }

    public void PopulatePanel()
    {
        ClearPanel();

        var source = GetSource();
        if (source == null)
        {
            return;
        }

        var built = new List<BaseElement<TData>>();
        foreach (var data in source)
        {
            var element = BuildElement(data);
            if (element == null)
            {
                continue;
            }
            if (!MatchesFilter(data))
            {
                continue;
            }

            if (IsGridModeEnabled)
            {
                element.Width.Set(GridCellWidth, 0f);
                element.Height.Set(GridCellHeight, 0f);
                element.MarginLeft = GridCellPadding;
                element.MarginTop = GridCellPadding;
                element.MarginRight = GridCellPadding;
                element.MarginBottom = GridCellPadding;
            }
            else
            {
                element.Width.Set(0, 1f);
                element.Height.Set(30, 0f);
                element.MarginLeft = 0f;
                element.MarginTop = 0f;
                element.MarginRight = 0f;
                element.MarginBottom = 0f;
            }

            items.Add(element);
            built.Add(element);
        }

        if (IsGridModeEnabled)
        {
            if (list.Parent == this)
            {
                list.Remove();
            }
            if (grid.Parent != this)
            {
                Append(grid);
            }

            grid.Clear();
            grid.AddRange(new List<UIElement>(built));
            grid.Recalculate();
        }
        else
        {
            if (grid.Parent == this)
            {
                grid.Remove();
            }
            if (list.Parent != this)
            {
                Append(list);
            }

            list.Clear();
            list.AddRange(new List<UIElement>(built));
            list.Recalculate();
        }

        Recalculate();
    }

    #region Filter
    protected virtual bool MatchesFilter(TData data)
    {
        string tag = GetTag(data) ?? string.Empty;

        string text = Main.chatText ?? string.Empty;
        if (text.Length == 0) return true;

        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        string prefix =
            this is CommandPanel ? "/" :
            this is ColorPanel ? "[c" :
            this is EmojiPanel ? "[e" :
            this is GlyphPanel ? "[g" :
            this is ItemPanel ? "[i" :
            this is ModIconPanel ? "[m" :
            this is PlayerIconPanel ? "[p" :
            this is UploadPanel ? "[u" :
            this is MentionPanel ? "@" : string.Empty;

        char bare = '\0';
        if (this is CommandPanel) bare = '/';
        if (this is EmojiPanel) bare = ':';
        if (this is UploadPanel) bare = '#';
        if (this is PlayerIconPanel) bare = '@';
        if (this is MentionPanel) bare = '@';

        string query = ExtractQuery(text, caret, prefix, bare);
        if (string.IsNullOrEmpty(query)) return true;

        if (this is EmojiPanel && data is Emoji emoji)
        {
            if (Contains(tag, query)) return true;
            if (emoji.Synonyms != null)
            {
                foreach (string syn in emoji.Synonyms)
                {
                    if (!string.IsNullOrEmpty(syn))
                    {
                        if (Contains(syn, query)) return true;
                    }
                }
            }
            return false;
        }

        if (this is ItemPanel && data is ItemEntry item)
        {
            if (Contains(tag, query)) return true;
            string dn = item.DisplayName ?? string.Empty;
            if (Contains(dn, query)) return true;
            if (int.TryParse(query, out int qid) && qid == item.ID) return true;
            return false;
        }

        return Contains(tag, query);


        static bool Contains(string haystack, string needle)
        {
            if (haystack == null) return false;
            return haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
        }

        static string ExtractQuery(string source, int caretPos, string pre, char bareChar)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;

            // 1) Bare prefix outside tags (e.g., :, #, @, /)
            if (bareChar != '\0')
            {
                int bi = LastIndexOf(source, bareChar, caretPos - 1);
                if (bi >= 0 && IsOutsideTags(source, bi))
                {
                    int start = bi + 1;
                    int end = FindStop(source, start);
                    if (end < 0 || end > caretPos) end = caretPos;
                    if (end <= start) return string.Empty;
                    return source.Substring(start, end - start).Trim();
                }
            }

            // 2) Bracketed or explicit string prefix (e.g., [e, [u, [p, /, @)
            if (!string.IsNullOrEmpty(pre))
            {
                int startIndex = Math.Max(0, Math.Min(caretPos - 1, source.Length - 1));
                int count = startIndex + 1;
                int s = source.LastIndexOf(pre, startIndex, count, StringComparison.OrdinalIgnoreCase);
                if (s >= 0)
                {
                    int start = s + pre.Length;

                    if (start < source.Length)
                    {
                        char ch = source[start];
                        if (ch == ':' || ch == '/')
                        {
                            start++;
                        }
                    }

                    int end = FindStop(source, start);
                    if (end < 0 || end > caretPos) end = caretPos;
                    if (end <= start) return string.Empty;

                    return source.Substring(start, end - start).Trim();
                }
            }

            return string.Empty;
        }

        static int LastIndexOf(string s, char ch, int fromInclusive)
        {
            if (fromInclusive < 0) return -1;
            int start = Math.Min(fromInclusive, s.Length - 1);
            return s.LastIndexOf(ch, start);
        }

        static int FindStop(string s, int start)
        {
            if (start >= s.Length) return -1;
            char[] stopChars = [' ', '\t', '\n', '\r', ']'];
            return s.IndexOfAny(stopChars, start);
        }

        static bool IsOutsideTags(string s, int indexBefore)
        {
            int lb = s.LastIndexOf('[', indexBefore);
            int rb = s.LastIndexOf(']', indexBefore);
            return lb <= rb;
        }
    }

    #endregion
    private bool _initialLeftSet;
    public override void Update(GameTime gt)
    {
        base.Update(gt);

        // Top set
        int itemCount = (int)(Conf.C?.AutocompleteItemsVisible ?? 10f);
        Top.Set(-64, 0);
        float desiredHeight = itemCount * 30f;
        if (Math.Abs(desiredHeight - _lastPanelHeightPx) > 0.1f)
        {
            Height.Set(desiredHeight, 0f);
            Recalculate();
            _lastPanelHeightPx = desiredHeight;
        }

        // Update viewmode
        UpdateViewmode();

        // Handle navigation keys
        if (IsGridModeEnabled)
            HandleGridNavigationKeys(gt);
        else
            HandleListNavigationKeys(gt);

        // Handle key presses
        HandleKeyPressed();
        HandleTabKeyPressed();
    }

    #region Viewmode
    private int gridSwitchSuppressFrames;
    public bool IsGridSwitchSuppressed => gridSwitchSuppressFrames > 0;
    private void UpdateViewmode()
    {
        if (CurrentViewMode != appliedViewMode)
        {
            bool switchingToGrid = CurrentViewMode == Viewmode.Grid;

            appliedViewMode = CurrentViewMode;

            PopulatePanel();

            if (IsGridModeEnabled)
            {
                if (grid._scrollbar != null)
                    grid._scrollbar.ViewPosition = 0f;
            }
            else
                list.ViewPosition = 0f;

            if (items.Count > 0)
                SetSelectedIndex(Math.Clamp(currentIndex, 0, items.Count - 1));

            if (switchingToGrid)
            {
                gridSwitchSuppressFrames = 0; // adjust to taste. 1-10 is reasonable
            }
        }
        if (gridSwitchSuppressFrames > 0)
        {
            //Log.Debug(gridSwitchSuppressFrames);
            gridSwitchSuppressFrames--;
        }
    }
    #endregion

    #region Navigation

    public void SetSelectedIndex(int index)
    {
        if (items.Count == 0) return;

        if (index < 0) index = items.Count - 1;
        else if (index >= items.Count) index = 0;

        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetSelected(false);
        }

        //Log.Debug(index);
        currentIndex = index;
        var current = items[currentIndex];
        current.SetSelected(true);
        string tag = GetTag(current.Data);

        if (IsGridModeEnabled)
        {
            grid.Recalculate();

            float viewportH = grid.GetInnerDimensions().Height;
            float rowHeight = GridCellHeight + grid.ListPadding;

            int rowIndex = currentIndex / GridColumns;
            float yTop = rowIndex * rowHeight;
            float yBottom = yTop + rowHeight;

            float view = 0f;
            if (grid._scrollbar != null)
            {
                view = grid._scrollbar.ViewPosition;
            }

            if (yTop < view)
            {
                view = yTop;
            }
            else if (yBottom > view + viewportH)
            {
                view = yBottom - viewportH;
            }

            float totalH = grid.GetTotalHeight();
            float maxView = Math.Max(0f, totalH - viewportH);

            if (grid._scrollbar != null)
            {
                grid._scrollbar.ViewPosition = MathHelper.Clamp(view, 0f, maxView);
            }
        }
        else
        {
            list.Recalculate();

            float viewportH = list.GetInnerDimensions().Height;
            float pad = list.ListPadding;

            if (viewportH <= 1f)
            {
                list.ViewPosition = 0f;
            }
            else
            {
                float yTop = 0f;
                for (int i = 0; i < currentIndex; i++)
                {
                    yTop += items[i].GetOuterDimensions().Height + pad;
                }

                float itemH = items[currentIndex].GetOuterDimensions().Height;
                float yBottom = yTop + itemH;

                float view = list.ViewPosition;

                if (yTop < view)
                {
                    view = yTop;
                }
                else if (yBottom > view + viewportH)
                {
                    view = yBottom - viewportH;
                }

                float totalH = 0f;
                for (int i = 0; i < items.Count; i++)
                {
                    totalH += items[i].GetOuterDimensions().Height + pad;
                }

                float maxView = Math.Max(0f, totalH - viewportH);
                list.ViewPosition = MathHelper.Clamp(view, 0f, maxView);
            }
        }

        if (ConnectedPanel is DescriptionPanel<TData> descPanel)
        {
            if (typeof(TData) == typeof(Upload))
            {
                return;
            }

            var element = items[currentIndex];
            if (element != null)
            {
                string desc = GetDescription(element.Data);

                if (element.Data is Emoji emoji && emoji.Synonyms.Count > 0)
                {
                    descPanel.SetText(string.Join(", ", emoji.Synonyms));
                }
                else
                {
                    descPanel.SetText(desc);
                }
            }

            if (descPanel.ConnectedPanel.GetType() == typeof(ColorPanel))
            {
                descPanel.GetText()._color = PlayerColorHandler.HexToColor(tag);
            }
        }

        // Ensure we don't start scrolled past top on first select
        if (currentIndex == 0) list.ViewPosition = 0f;
    }

    private void HandleKeyPressed()
    {
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (!JustPressed(key))
                continue;

            if (key == Keys.Tab ||
                key == Keys.Up ||
                key == Keys.Down ||
                key == Keys.LeftControl || key == Keys.LeftShift)
                return;

            int prevIndex = currentIndex;
            PopulatePanel();

            // ensure something is still selected
            if (items.Count > 0)
            {
                int newIndex = Math.Clamp(prevIndex, 0, items.Count - 1);
                SetSelectedIndex(newIndex);
            }
            else
            {
                if (ConnectedPanel is DescriptionPanel<TData> descPanel)
                {
                    descPanel.SetText("No entries found.");
                }
            }
        }
    }

    private void HandleTabKeyPressed()
    {
        if (!JustPressed(Keys.Tab) || items.Count == 0 || currentIndex < 0)
            return;

        InsertSelectedTag();

        if (this is ColorPanel)
        {
            Main.chatText += "]";
        }
    }

    /// <summary>
    /// Handle inserting most tags. 
    /// Special cases are overriden in their separate panels for commands, emojis, mentions, etc.
    /// </summary>
    public virtual void InsertSelectedTag()
    {
        if (items.Count == 0 || currentIndex < 0) return;

        string tag = GetTag(items[currentIndex].Data);
        if (string.IsNullOrEmpty(tag)) return;

        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        // panel-specific prefix to detect the fragment/token
        string prefix = this switch
        {
            ColorPanel => "[c",
            GlyphPanel => "[g",
            ItemPanel => "[i",
            ModIconPanel => "[m",
            PlayerIconPanel => "[p",
            UploadPanel => "[u",
            _ => "[e"
        };

        // find the last prefix before/at the caret
        int start = text.LastIndexOf(prefix, Math.Max(0, caret - 1), StringComparison.OrdinalIgnoreCase);

        if (start >= 0)
        {
            // Is there already a closing bracket for that token?
            int close = text.IndexOf(']', start);

            // CASE A: Open fragment or caret is inside the token => REPLACE that fragment/token
            // (close == -1 -> still typing; or caret <= close+1 -> caret inside or at end of token)
            if (close == -1 || caret <= close + 1)
            {
                int replaceEnd = (close == -1) ? caret : close + 1; // include ']'
                string before = text.Substring(0, start);
                string after = text.Substring(replaceEnd);

                // Avoid piling spaces after repeated replacements
                after = after.TrimStart();

                Main.chatText = before + tag + after;

                // Put caret right after the inserted tag
                HandleChatSystem.SetCaretPos((before + tag).Length);
                return;
            }

            // CASE B: caret is past the finished token => append a new one, preserving spacing
            {
                bool needSpace = (text.Length > 0 && !char.IsWhiteSpace(text[text.Length - 1]));
                Main.chatText = text + (needSpace ? " " : "") + tag + " ";
                HandleChatSystem.SetCaretPos(Main.chatText.Length);
                return;
            }
        }

        // No fragment found -> just append with clean spacing
        {
            bool needSpace = (text.Length > 0 && !char.IsWhiteSpace(text[text.Length - 1]));
            Main.chatText = text + (needSpace ? " " : "") + tag + " ";
            HandleChatSystem.SetCaretPos(Main.chatText.Length);
        }
    }

    private void HandleListNavigationKeys(GameTime gt)
    {
        // Tap key
        if (JustPressed(Keys.Up))
        {
            SetSelectedIndex(currentIndex - 1);
            heldKey = Keys.Up;
            repeatTimer = 0.35;
        }
        else if (JustPressed(Keys.Down))
        {
            SetSelectedIndex(currentIndex + 1);
            heldKey = Keys.Down;
            repeatTimer = 0.35;
        }

        // Hold key to repeat navigation
        double dt = gt.ElapsedGameTime.TotalSeconds;
        if (Main.keyState.IsKeyDown(heldKey))
        {
            repeatTimer -= dt;
            if (repeatTimer <= 0)
            {
                repeatTimer += 0.06;

                if (Main.keyState.IsKeyDown(Keys.Up))
                {
                    SetSelectedIndex(currentIndex - 1);
                }
                else if (Main.keyState.IsKeyDown(Keys.Down))
                {
                    SetSelectedIndex(currentIndex + 1);
                }
            }
        }
    }
    private void HandleGridNavigationKeys(GameTime gt)
    {
        int cols = GridColumns;
        int total = items.Count;
        if (cols <= 0 || total <= 0 || currentIndex < 0) return;

        int currentRow = currentIndex / cols;
        int lastRow = (total - 1) / cols;

        bool moved = false;

        if (JustPressed(Keys.Left))
        {
            int col = currentIndex % cols;
            if (col > 0)
            {
                SetSelectedIndex(currentIndex - 1);
                heldKey = Keys.Left;
                repeatTimer = 0.35;
                moved = true;
            }
        }
        else if (JustPressed(Keys.Right))
        {
            int col = currentIndex % cols;
            bool hasNext = currentIndex < total - 1;
            if (col < cols - 1 && hasNext)
            {
                SetSelectedIndex(currentIndex + 1);
                heldKey = Keys.Right;
                repeatTimer = 0.35;
                moved = true;
            }
        }
        else if (JustPressed(Keys.Up))
        {
            if (currentIndex - cols >= 0)
            {
                SetSelectedIndex(currentIndex - cols);
                heldKey = Keys.Up;
                repeatTimer = 0.35;
                moved = true;
            }
        }
        else if (JustPressed(Keys.Down))
        {
            // Clamp at bottom: only move if not on last row and target exists
            int candidate = currentIndex + cols;
            if (currentRow < lastRow && candidate < total)
            {
                SetSelectedIndex(candidate);
                heldKey = Keys.Down;
                repeatTimer = 0.35;
                moved = true;
            }
        }

        double dt = gt.ElapsedGameTime.TotalSeconds;

        if (moved == false && heldKey != Keys.None && Main.keyState.IsKeyDown(heldKey))
        {
            repeatTimer -= dt;
            if (repeatTimer <= 0)
            {
                repeatTimer += 0.06;

                if (Main.keyState.IsKeyDown(Keys.Left))
                {
                    int col = currentIndex % cols;
                    if (col > 0)
                        SetSelectedIndex(currentIndex - 1);
                }
                else if (Main.keyState.IsKeyDown(Keys.Right))
                {
                    int col = currentIndex % cols;
                    bool hasNext = currentIndex < total - 1;
                    if (col < cols - 1 && hasNext)
                        SetSelectedIndex(currentIndex + 1);
                }
                else if (Main.keyState.IsKeyDown(Keys.Up))
                {
                    if (currentIndex - cols >= 0)
                        SetSelectedIndex(currentIndex - cols);
                }
                else if (Main.keyState.IsKeyDown(Keys.Down))
                {
                    int candidate = currentIndex + cols;
                    int row = currentIndex / cols;
                    int last = (total - 1) / cols;

                    if (row < last && candidate < total)
                        SetSelectedIndex(candidate);
                }
            }
        }
    }

    #endregion
}