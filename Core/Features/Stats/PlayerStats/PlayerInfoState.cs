using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChatPlus.Core.Features.Stats.Base;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ChatPlus.Core.Features.Stats.PlayerStats;

public class PlayerInfoState : BaseInfoState, ILoadable
{
    public static PlayerInfoState instance;

    private UIElement _messageBox;
    private static Type _messageBoxType;

    // nav in bottom bar
    private UITextPanel<string> _prevBtn;
    private UITextPanel<string> _nextBtn;

    private int _currentPlayerIndex;
    private int _whoAmI;
    private string _playerName = "Unknown";
    public static bool Active { get; private set; }

    public void Load(Mod mod)
    {
        instance = this;
        _messageBoxType = typeof(UICommon).Assembly.GetType("Terraria.ModLoader.UI.UIMessageBox");
    }

    public void Unload()
    {
        instance = null;
    }

    #region Cycle players
    public override void OnInitialize()
    {
        // Build the shared chrome (Root, MainPanel, TitlePanel, BottomBar, BackButton)
        base.OnInitialize();

        // Optional add own inner "body" to MainPanel
        var body = new UIPanel
        {
            Width = { Pixels = -25f, Percent = 1f },
            Height = { Percent = 1f },
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent
        };
        MainPanel.Append(body);

        if (_messageBoxType != null)
        {
            _messageBox = (UIElement)Activator.CreateInstance(
                _messageBoxType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { string.Empty },
                null);
            _messageBox.Width.Set(0, 1f);
            _messageBox.Height.Set(0, 1f);
            body.Append(_messageBox);
        }

        _currentPlayerIndex = _whoAmI >= 0 ? _whoAmI : Main.myPlayer;
        SetPlayerFromIndex(_currentPlayerIndex);
    }

    public override void OnActivate() => Active = true;
    public override void OnDeactivate() => Active = false;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        RefreshNavButtons();
    }

    private void RefreshNavButtons()
    {
        const float NAV_W = 36f;

        int[] active = GetActivePlayerIndices();
        bool multi = Main.netMode != NetmodeID.SinglePlayer && active.Length > 1;

        if (multi)
        {
            if (_prevBtn == null)
            {
                _prevBtn = new UITextPanel<string>("<", 1.0f, false)
                {
                    Width = { Pixels = NAV_W },
                    Height = { Pixels = 40f }
                }.WithFadedMouseOver();
                _prevBtn.Left.Set(0f, 0f);
                _prevBtn.OnLeftClick += (_, __) => CyclePlayer(-1);
            }

            if (_nextBtn == null)
            {
                _nextBtn = new UITextPanel<string>(">", 1.0f, false)
                {
                    Width = { Pixels = NAV_W },
                    Height = { Pixels = 40f },
                    HAlign = 1f
                }.WithFadedMouseOver();
                _nextBtn.Left.Set(0f, 0f);
                _nextBtn.OnLeftClick += (_, __) => CyclePlayer(+1);
            }
        }

        if (!multi)
        {
            if (_prevBtn?.Parent != null) _prevBtn.Remove();
            if (_nextBtn?.Parent != null) _nextBtn.Remove();

            // Stretch the base BackButton across BottomBar
            BackButton.Left.Set(0f, 0f);
            BackButton.Width.Set(0f, 1f);

            BackButton.Recalculate();
            BottomBar.Recalculate();
            Recalculate();
            return;
        }

        int pos = Array.IndexOf(active, _currentPlayerIndex);
        if (pos < 0)
        {
            pos = 0;
        }

        bool hasPrev = pos > 0;
        bool hasNext = pos < active.Length - 1;

        if (hasPrev)
        {
            if (_prevBtn.Parent == null) BottomBar.Append(_prevBtn);
        }
        else if (_prevBtn?.Parent != null) _prevBtn.Remove();

        if (hasNext)
        {
            if (_nextBtn.Parent == null) BottomBar.Append(_nextBtn);
        }
        else if (_nextBtn?.Parent != null) _nextBtn.Remove();

        float leftW = hasPrev ? NAV_W : 0f;
        float rightW = hasNext ? NAV_W : 0f;

        // Position the base BackButton between prev/next
        BackButton.Left.Set(leftW, 0f);
        BackButton.Width.Set(-(leftW + rightW), 1f);

        _prevBtn?.Recalculate();
        _nextBtn?.Recalculate();
        BackButton.Recalculate();
        BottomBar.Recalculate();
        Recalculate();
    }
    private void CyclePlayer(int dir)
    {
        int[] list = GetActivePlayerIndices();
        if (list.Length == 0)
        {
            return;
        }

        int pos = Array.IndexOf(list, _currentPlayerIndex);
        if (pos < 0)
        {
            pos = 0;
        }

        if (dir < 0 && pos > 0)
        {
            SetPlayerFromIndex(list[pos - 1]);
        }
        else if (dir > 0 && pos < list.Length - 1)
        {
            SetPlayerFromIndex(list[pos + 1]);
        }
    }

    private void SetPlayerFromIndex(int idx)
    {
        if (idx < 0 || idx >= Main.maxPlayers)
        {
            return;
        }

        Player p = Main.player[idx];
        if (p == null || !p.active)
        {
            return;
        }

        _currentPlayerIndex = idx;
        _whoAmI = idx;

        _playerName = p.name;
        SetTitle($"Player: {_playerName}");
    }

    private static int[] GetActivePlayerIndices()
    {
        var list = new List<int>(Main.maxPlayers);
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p != null && p.active)
            {
                list.Add(i);
            }
        }
        return list.ToArray();
    }

    public void SetPlayer(int whoAmI, string nameOverride = null)
    {
        _whoAmI = whoAmI;
        if (!string.IsNullOrEmpty(nameOverride))
        {
            _playerName = nameOverride;
        }
        else
        {
            if (Main.player != null && whoAmI >= 0 && whoAmI < Main.player.Length && Main.player[whoAmI] != null)
            {
                _playerName = Main.player[whoAmI].name;
            }
            else
            {
                _playerName = $"Player {whoAmI}";
            }
        }
    }

    #endregion

    #region Draw
    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);

        var player = _whoAmI >= 0 && _whoAmI < Main.maxPlayers ? Main.player[_whoAmI] : null;
        if (player == null || !player.active) return;

        int panelWidth = 96, gutter = 10, rowHeight = 31;
        int containerW = Math.Min((int)(Main.screenWidth * 0.8f), 1000);
        int containerLeft = (Main.screenWidth - containerW) / 2, top = 120;
        int bgW = 436, bgH = 200;
        var bgRect = new Rectangle(containerLeft + 42, top + 42, bgW, bgH);
        int leftColumn = containerLeft + containerW - (panelWidth * 2 + gutter) - 60;
        int rightColumn = leftColumn + panelWidth + gutter;
        int y0 = bgRect.Y;
        bool smallWidth = Main.screenWidth < 1200;
        if (Main.screenWidth < 940) bgRect.Width = bgW - 110;

        int panelTop = top;
        int panelBottom = Main.screenHeight - 110;

        int invSlot = smallWidth ? 36 : 40;
        int invPad = 4;
        int invRowH = invSlot + invPad;
        int invRows = 5;
        int invHeight = invRows * invRowH;

        int buffSize = 32, buffPad = 6, buffPerRow = 10;
        int buffCount = CountActiveBuffs(player);
        int buffRows = (buffCount + buffPerRow - 1) / buffPerRow;
        int buffRowH = buffSize + buffPad;
        int buffHeight = buffRows * buffRowH;

        int accSlot = smallWidth ? 36 : 40;
        int accPad = 4;
        int accRowH = accSlot + accPad;
        int dyeRows = Math.Max(0, player.dye.Length - 3);
        int equipRows = Math.Max(0, player.armor.Length - 3);
        int vanityRows = Math.Max(0, player.armor.Length - 13);
        int accRows = Math.Min(dyeRows, Math.Min(equipRows, vanityRows));
        int accHeight = accRows * accRowH;

        var playerPos = new Vector2(bgRect.X + bgRect.Width * 0.5f - 100f, bgRect.Y + 50);

        Vector2 invStart = new(bgRect.X, bgRect.Bottom + 52);
        Vector2 buffStart = new(bgRect.X, invStart.Y + invHeight + 35);

        int xOffset = 0, yOffset = 0;
        if (smallWidth) { xOffset = 275; yOffset = 220; }

        Vector2 accPos = new(
            x: leftColumn - 200 + xOffset,
            y: y0 + 30 + yOffset
        );

        PlayerInfoDrawer.DrawSeparatorBorder(sb, bgRect);
        PlayerInfoDrawer.DrawMapFullscreenBackground(sb, bgRect, player);

        // Draw stats to the left of player
        Vector2 bgTopLeft = new(bgRect.X, bgRect.Y);
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            PlayerInfoDrawer.DrawPlayerIDText(sb, bgTopLeft, player);
            bgTopLeft += new Vector2(0, 21);
            PlayerInfoDrawer.DrawTeamText(sb, bgTopLeft, player);
        }

        // Draw player
        PlayerInfoDrawer.DrawPlayer(sb, playerPos, player, 1.8f);

        Vector2 statsHeaderPos = new(leftColumn, y0 - 4);
        y0 -= 8;
        if (!smallWidth) Utils.DrawBorderStringBig(sb, Loc.Get("PlayerInfo.Headers.Stats"), statsHeaderPos, Color.White, scale: 0.5f);
        if (smallWidth) y0 -= 30;

        Rectangle hpBounds = new(leftColumn, y0 + rowHeight + 6, panelWidth, rowHeight);
        Rectangle manaBounds = new(rightColumn, y0 + rowHeight + 6, panelWidth, rowHeight);
        Rectangle defBounds = new(leftColumn, y0 + rowHeight * 2 + 12, panelWidth, rowHeight);
        Rectangle deathBounds = new(rightColumn, y0 + rowHeight * 2 + 12, panelWidth, rowHeight);
        Rectangle coinBounds = new(leftColumn, y0 + rowHeight * 3 + 18, panelWidth, rowHeight);
        Rectangle ammoBounds = new(rightColumn, y0 + rowHeight * 3 + 18, panelWidth, rowHeight);
        Rectangle minionBounds = new(leftColumn, y0 + rowHeight * 4 + 24, panelWidth, rowHeight);
        Rectangle sentryBounds = new(rightColumn, y0 + rowHeight * 4 + 24, panelWidth, rowHeight);
        Rectangle timeInSessionBounds = new(leftColumn, y0 + rowHeight * 5 + 30, panelWidth, rowHeight);
        Rectangle daysInSessionBounds = new(rightColumn, y0 + rowHeight * 5 + 30, panelWidth, rowHeight);
        Rectangle lastEnemyBounds = new(leftColumn, y0 + rowHeight * 6 + 36, panelWidth, rowHeight);
        Rectangle lastBossBounds = new(rightColumn, y0 + rowHeight * 6 + 36, panelWidth, rowHeight);

        PlayerInfoDrawer.DrawStat_HP(sb, hpBounds, player);
        PlayerInfoDrawer.DrawStat_Mana(sb, manaBounds, player);
        PlayerInfoDrawer.DrawStat_Defense(sb, defBounds, player);
        PlayerInfoDrawer.DrawStat_Attack(sb, deathBounds, player);
        PlayerInfoDrawer.DrawStat_Coins(sb, coinBounds, player);
        PlayerInfoDrawer.DrawStat_Ammo(sb, ammoBounds, player);
        PlayerInfoDrawer.DrawStat_Minions(sb, minionBounds, player);
        PlayerInfoDrawer.DrawStat_Sentries(sb, sentryBounds, player);
        PlayerInfoDrawer.DrawStat_TimeInSession(sb, timeInSessionBounds, player);
        PlayerInfoDrawer.DrawStat_DaysInSession(sb, daysInSessionBounds, player);
        //PlayerInfoDrawer.DrawStat_LastEnemyHit(sb, lastEnemyBounds, player);
        //PlayerInfoDrawer.DrawStat_LastBossHit(sb, lastBossBounds, player);

        Rectangle viewport = new(containerLeft + 20, top + 20, containerW - 20 * 4, panelBottom - top - 20 * 2);
        //DrawDebugRect(viewport);
        Utils.DrawBorderStringBig(sb, Loc.Get("PlayerInfo.Headers.Inventory"), new Vector2(invStart.X + 2, invStart.Y - 35), Color.White, 0.5f);
        DrawInventory(sb, invStart, player, viewport);
        DrawAccessories(sb, accPos, player, viewport);

        if (viewport.Bottom > buffStart.Y + 20)
        {
            string buffsHeader = Loc.Get("PlayerInfo.Headers.Buffs");

            if (PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
                buffsHeader += ": " + buffCount;

            Utils.DrawBorderStringBig(sb, buffsHeader, new Vector2(buffStart.X, buffStart.Y - 36), Color.White, 0.52f);
            DrawBuffs(sb, buffStart, player, viewport);
        }

        Point p = Main.MouseScreen.ToPoint();
        StatTooltip(hpBounds, p, "PlayerInfo.Stats.Health", player);
        StatTooltip(manaBounds, p, "PlayerInfo.Stats.Mana", player);
        StatTooltip(defBounds, p, "PlayerInfo.Stats.Defense", player);
        StatTooltip(deathBounds, p, "PlayerInfo.Stats.Attack", player);
        StatTooltip(coinBounds, p, "PlayerInfo.Stats.Coins", player);
        StatTooltip(ammoBounds, p, "PlayerInfo.Stats.Ammo", player);
        StatTooltip(minionBounds, p, "PlayerInfo.Stats.Minions", player);
        StatTooltip(sentryBounds, p, "PlayerInfo.Stats.Sentries", player);
        StatTooltip(timeInSessionBounds, p, "PlayerInfo.Stats.TimeInSession", player);
        StatTooltip(daysInSessionBounds, p, "PlayerInfo.Stats.DaysInSession", player);
        //StatTooltip(lastEnemyBounds, p, "PlayerInfo.Stats.LastEnemyHit", player);
        //StatTooltip(lastBossBounds, p, "PlayerInfo.Stats.LastBossHit", player);
    }

    private static void StatTooltip(Rectangle bounds, Point p, string loc, Player target)
    {
        if (bounds.Contains(p))
        {
            string localizedText = Loc.Get(loc);
            if (!PlayerInfoDrawer.HasAccess(Main.LocalPlayer, target))
                localizedText += " (Locked)";
            UICommon.TooltipMouseText(localizedText);
        }
    }


    private static void DrawLockSlot(SpriteBatch sb, Vector2 pos, float scale = 1.5f)
    {
        Texture2D tex = TextureAssets.HbLock[0].Value;

        // Calculate the width of one frame (assuming horizontal split)
        int frameWidth = tex.Width / 2;
        Rectangle sourceRect = new(0, 0, frameWidth, tex.Height);

        sb.Draw(tex, pos, sourceRect, Color.White, 0f, Vector2.Zero,
            scale: scale, SpriteEffects.None, 0f);
    }

    private static void DrawInventory(SpriteBatch sb, Vector2 start, Player player, Rectangle viewport)
    {


        int size = Main.screenWidth < 1200 ? 36 : 40;
        int pad = 4;

        for (int i = 0; i < 50; i++)
        {
            int row = i / 10, col = i % 10;

            var r = new Rectangle(
                (int)(start.X + col * (size + pad)),
                (int)(start.Y + row * (size + pad)),
                size, size
            );

            if (r.Bottom > viewport.Bottom - 10) continue;

            var item = player.inventory[i];

            if (item == player.HeldItem)
                sb.Draw(TextureAssets.InventoryBack14.Value, r, Color.White);
            else
                sb.Draw(TextureAssets.InventoryBack.Value, r, Color.White);

            Vector2 slotPos = new(r.X + 7, r.Y + 7);
            //DrawLockSlot(sb, slotPos, 1.0f);

            if (!item.IsAir)
            {
                var center = new Vector2(r.X + r.Width / 2f, r.Y + r.Height / 2f);
                if (!PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
                {
                    DrawLockSlot(sb, slotPos, 1.0f);
                }
                else
                {
                    ItemSlot.DrawItemIcon(item, 31, sb, center, 0.9f, size - 6, Color.White);
                }

                if (r.Contains(Main.MouseScreen.ToPoint()))
                {
                    UICommon.TooltipMouseText("");
                    Main.LocalPlayer.mouseInterface = true;
                    if (!PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
                    {
                        UICommon.TooltipMouseText("Locked");
                        Main.hoverItemName = "Locked";
                    }
                    else
                    {
                        Main.HoverItem = item.Clone();
                        Main.hoverItemName = item.Name;
                    }

                    if (Main.mouseRight && Main.mouseRightRelease && !PlayerInput.IgnoreMouseInterface)
                    {
                        bool TryEquipFromInventory(Player p, int invIndex)
                        {
                            if (player != Main.LocalPlayer) return false;
                            ref Item it = ref p.inventory[invIndex]; if (it.IsAir) return false;

                            if (it.accessory)
                            {
                                int begin = 3;
                                int end = Math.Min(10, p.armor.Length);
                                int empty = -1;
                                for (int s = begin; s < end; s++)
                                {
                                    if (p.armor[s].type == it.type) return false;
                                    if (empty < 0 && p.armor[s].IsAir) empty = s;
                                }
                                if (empty >= 0) { p.armor[empty] = it.Clone(); it.TurnToAir(); return true; }
                                for (int s = begin; s < end; s++)
                                {
                                    if (!p.armor[s].IsAir)
                                    {
                                        var tmp = p.armor[s];
                                        p.armor[s] = it;
                                        p.inventory[invIndex] = tmp;
                                        return true;
                                    }
                                }
                                return false;
                            }

                            int equip = -1, vanity = -1;
                            if (it.headSlot >= 0) { equip = 0; vanity = 10; }
                            else if (it.bodySlot >= 0) { equip = 1; vanity = 11; }
                            else if (it.legSlot >= 0) { equip = 2; vanity = 12; }

                            if (equip >= 0)
                            {
                                int target = it.defense > 0 ? equip : vanity;
                                if (p.armor[target].IsAir) { p.armor[target] = it.Clone(); it.TurnToAir(); return true; }
                                var sw = p.armor[target]; p.armor[target] = it; p.inventory[invIndex] = sw; return true;
                            }
                            return false;
                        }

                        if (TryEquipFromInventory(player, i))
                            Main.mouseRightRelease = false;
                    }
                }
            }

            if (i < 10)
            {
                string label = i == 9 ? "0" : (i + 1).ToString();
                Vector2 numberPos = new(r.Right - 32f, r.Bottom - 36f);
                Utils.DrawBorderString(sb, label, numberPos, Color.White, 0.75f, 0f, 0f);
            }
        }
    }

    private static void DrawLoaderSlot(Item[] items, int context, int slot, int x, int y, Player player)
    {
        float old = Main.inventoryScale;
        Main.inventoryScale = 40f / TextureAssets.InventoryBack.Width();
        bool smallWidth = Main.screenWidth < 1200;
        if (smallWidth) Main.inventoryScale = 36f / TextureAssets.InventoryBack.Width();

        int w = (int)(TextureAssets.InventoryBack.Width() * Main.inventoryScale);
        int h = (int)(TextureAssets.InventoryBack.Height() * Main.inventoryScale);
        bool hover = new Rectangle(x, y, w, h).Contains(Main.MouseScreen.ToPoint()) && !PlayerInput.IgnoreMouseInterface;

        ref Item it = ref items[slot];

        // If the slot has an item but access is denied → show lock instead of item
        if (!it.IsAir && !PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
        {
            Item dummy = new Item();
            dummy.SetDefaults(ItemID.None); // force blank slot
            ItemSlot.Draw(Main.spriteBatch, ref dummy, context, new Vector2(x, y));
            DrawLockSlot(Main.spriteBatch, new Vector2(x + 7, y + 7), 1f);
            Main.inventoryScale = old;
            if (!it.IsAir && hover)
            {
                UICommon.TooltipMouseText("Locked");
            }
            return;
        }

        // Normal access OR slot is empty
        if (!it.IsAir && hover)
        {
            UICommon.TooltipMouseText("");
            Main.LocalPlayer.mouseInterface = true;
            ItemSlot.OverrideHover(items, context, slot);

            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;
            }
            if (Main.mouseRight && Main.mouseRightRelease && player == Main.LocalPlayer)
            {
                ItemSlot.RightClick(items, context, slot);
                Main.mouseRightRelease = false;
            }

            ItemSlot.MouseHover(items, context, slot);
        }

        ItemSlot.Draw(Main.spriteBatch, ref it, context, new Vector2(x, y));
        Main.inventoryScale = old;
    }

    private static void DrawAccessories(SpriteBatch sb, Vector2 topLeft, Player player, Rectangle viewport)
    {
        int size = Main.screenWidth < 1200 ? 36 : 40;
        int pad = 4;
        int rowStep = size + pad;

        Utils.DrawBorderStringBig(sb, "Accessories", new Vector2(topLeft.X, topLeft.Y - 36), Color.White, 0.52f);

        // Armor rows (head/body/legs + dyes + vanity)
        for (int r = 0; r < 3; r++)
        {
            int y = (int)topLeft.Y + r * rowStep;

            int x0 = (int)topLeft.X + 0 * rowStep;
            int x1 = (int)topLeft.X + 1 * rowStep;
            int x2 = (int)topLeft.X + 2 * rowStep;

            // draw 3 armor dye slots
            DrawLoaderSlot(player.dye, ItemSlot.Context.EquipDye, r, x0, y, player);

            // draw 3 vanity slots
            DrawLoaderSlot(player.armor, ItemSlot.Context.InWorld, 10 + r, x1, y, player);

            // draw 3 armor slots
            DrawLoaderSlot(player.armor, ItemSlot.Context.InWorld, r, x2, y, player);

            // backup visuals
            float scaleBackup = Main.inventoryScale;
            Main.inventoryScale = size / (float)TextureAssets.InventoryBack.Width();

            if (PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
            {
                ItemSlot.Draw(sb, player.armor, ItemSlot.Context.EquipArmorVanity, 10 + r, new Vector2(x1, y));
                ItemSlot.Draw(sb, player.armor, ItemSlot.Context.EquipArmor, r, new Vector2(x2, y));
            }
            else
            {
                Item[] _ghostArmor = Enumerable.Repeat(new Item(), 30).ToArray();
                Item[] _ghostDye = Enumerable.Repeat(new Item(), 10).ToArray();
                ItemSlot.Draw(sb, _ghostArmor, ItemSlot.Context.EquipArmorVanity, 10 + r, new Vector2(x1, y));
                ItemSlot.Draw(sb, _ghostArmor, ItemSlot.Context.EquipArmor, r, new Vector2(x2, y));

                // overlay locks ONLY if the real player slots contain items
                if (10 + r < player.armor.Length && !player.armor[10 + r].IsAir)
                    DrawLockSlot(sb, new Vector2(x1 + 7, y + 7), 1.0f);

                if (r < player.armor.Length && !player.armor[r].IsAir)
                    DrawLockSlot(sb, new Vector2(x2 + 7, y + 7), 1.0f);
            }


            Main.inventoryScale = scaleBackup;

            bool SlotHasItem(Item[] arr, int index)
                => index >= 0 && index < arr.Length && !arr[index].IsAir;
        }

        // Accessories rows
        Vector2 accTopLeft = new(topLeft.X, topLeft.Y + 3 * rowStep);

        int dyeRows = Math.Max(0, player.dye.Length - 3);
        int equipRows = Math.Max(0, player.armor.Length - 3);
        int vanityRows = Math.Max(0, player.armor.Length - 13);
        int totalRows = Math.Min(dyeRows, Math.Min(equipRows, vanityRows));

        for (int r = 0; r < totalRows; r++)
        {
            int y = (int)accTopLeft.Y + r * rowStep;
            if (y + size > viewport.Bottom) break;

            int dyeIndex = 3 + r;
            int vanityIndex = 13 + r;
            int equipIndex = 3 + r;

            int x0 = (int)accTopLeft.X + 0 * rowStep;
            int x1 = (int)accTopLeft.X + 1 * rowStep;
            int x2 = (int)accTopLeft.X + 2 * rowStep;

            // draw accessory dye slots
            DrawLoaderSlot(player.dye, ItemSlot.Context.EquipDye, dyeIndex, x0, y, player);

            // draw accessory vanity slots
            DrawLoaderSlot(player.armor, ItemSlot.Context.EquipAccessoryVanity, vanityIndex, x1, y, player);

            // draw accessory slots
            DrawLoaderSlot(player.armor, ItemSlot.Context.EquipAccessory, equipIndex, x2, y, player);
        }
    }

    private static void DrawBuffs(SpriteBatch sb, Vector2 start, Player player, Rectangle viewport)
    {
        int size = 32;
        int pad = 6;
        int perRow = 10;
        float textScale = 0.75f;
        var font = FontAssets.MouseText.Value;
        int n = 0;

        for (int i = 0; i < player.buffType.Length; i++)
        {
            int id = player.buffType[i];
            if (id <= 0) continue;
            if (!player.HasBuff(id)) continue;
            if (id >= TextureAssets.Buff.Length) continue;

            int row = n / perRow;
            int col = n % perRow;

            int x = (int)(start.X + col * (size + pad));
            int y = (int)(start.Y + row * (size + pad));

            if (y + size > viewport.Bottom) break;

            var iconRect = new Rectangle(x, y, size, size);

            bool showTime = player.buffTime[i] > 2 && !Main.buffNoTimeDisplay[id];
            string label = string.Empty;
            Vector2 timePos = default;
            Rectangle textRect = Rectangle.Empty;

            if (showTime)
            {
                int seconds = player.buffTime[i] / 60;
                if (seconds >= 60)
                {
                    int minutes = seconds / 60;
                    label = minutes + " m";
                }
                else
                {
                    label = seconds + " s";
                }

                int xOffset = 0;
                if (seconds <= 600 && seconds >= 60) xOffset = 4;
                if (seconds <= 60) xOffset = 2;
                if (seconds <= 10) xOffset = 3;
                if (seconds <= 9) xOffset = 6;

                timePos = new Vector2(iconRect.X + xOffset, iconRect.Bottom + 2);

                Vector2 textSize = font.MeasureString(label) * textScale;
                textRect = new Rectangle((int)timePos.X, (int)timePos.Y, (int)Math.Ceiling(textSize.X), (int)Math.Ceiling(textSize.Y));
            }

            Point mouse = Main.MouseScreen.ToPoint();
            bool hoverIcon = iconRect.Contains(mouse) && !PlayerInput.IgnoreMouseInterface;
            bool hoverText = showTime && textRect.Contains(mouse) && !PlayerInput.IgnoreMouseInterface;
            bool hover = hoverIcon || hoverText;

            float alpha = hover ? 1f : 0.75f;

            if (PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
            {
                sb.Draw(TextureAssets.Buff[id].Value, iconRect, Color.White * alpha);
            }
            else
            {
                if (TextureAssets.Buff.Length > 203 && TextureAssets.Buff[203] != null)
                {
                    sb.Draw(TextureAssets.Buff[203].Value, iconRect, Color.White * alpha);
                }
                DrawLockSlot(sb, new Vector2(iconRect.X + 5, iconRect.Y + 4), 1.0f);
            }


            if (showTime && PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
            {
                sb.DrawString(font, label, timePos, Color.White * alpha, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
            }

            if (hover)
            {
                string name = Lang.GetBuffName(id);
                string desc = Lang.GetBuffDescription(id);
                string tooltip = string.IsNullOrEmpty(desc) ? name : name + "\n" + desc;
                if (!PlayerInfoDrawer.HasAccess(Main.LocalPlayer, player))
                {
                    tooltip = "Locked";
                }

                Main.instance.MouseText(tooltip);
                Main.LocalPlayer.mouseInterface = true;

                if (Main.mouseRight && Main.mouseRightRelease && !Main.debuff[id] && 
                    Main.LocalPlayer != null && Main.LocalPlayer == player)
                {
                    player.DelBuff(i);
                    Main.mouseRightRelease = false;
                }
            }

            n++;
        }
    }

    #endregion

    #region Helpers
    private static int CountActiveBuffs(Player p)
    {
        int n = 0;
        for (int i = 0; i < p.buffType.Length; i++)
            if (p.buffType[i] > 0 && p.buffTime[i] > 0) n++;
        return n;
    }
    #endregion

    private static void DrawDebugRect(Rectangle r) => Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, r, Color.Red * 0.5f);
}
