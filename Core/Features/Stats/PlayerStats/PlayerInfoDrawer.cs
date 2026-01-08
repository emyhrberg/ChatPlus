using System;
using ChatPlus.Common;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Stats.PlayerStats.SessionTracker;
using ChatPlus.Core.Features.Stats.PlayerStats.StatsPrivacy;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Stats.PlayerStats;

public static class PlayerInfoDrawer
{
    /// <summary>
    /// Draws a big tooltip panel with player info, pokémon style.
    /// </summary>
    public static void Draw(SpriteBatch sb, Player player)
    {
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                 DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

        // Dimensions
        const int panelWidth = 96;
        const int gutter = 10;
        const int side = 7;
        const int H = 356;
        const int W = panelWidth * 2 + gutter + side * 2;
        const int rowHeight = 31;
        var pos = Main.MouseScreen + new Vector2(20, 20);
        //pos = new(300, 400); // debug
        pos.X = Math.Clamp(pos.X, 0, Main.screenWidth - W);
        pos.Y = Math.Clamp(pos.Y, 0, Main.screenHeight - H);
        var rect = new Rectangle((int)pos.X, (int)pos.Y, W, H);

        // Draw BG panel
        DrawFullBGPanel(sb, rect);

        DrawHeaderText(sb, rect, player);
        var cursor = pos + new Vector2(side, 32);
        DrawHorizontalSeparator(sb, cursor, W - side * 2);
        cursor += new Vector2(0, 10);
        rect = new Rectangle((int)cursor.X, (int)cursor.Y, W - side * 2, 100);
        DrawSeparatorBorder(sb, rect);

        // Draw background
        rect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
        DrawMapFullscreenBackground(sb, rect, player);

        // Draw player
        DrawPlayer(sb, pos, player);

        // Draw stats to the left of player
        if (Main.netMode != NetmodeID.SinglePlayer && player.team != 0)
        {
            DrawTeamText(sb, cursor, player); // team x
        }

        // Here is the end of player panel
        cursor += new Vector2(0, 110);
        DrawHorizontalSeparator(sb, cursor, W - side * 2);

        // Draw stats rows
        cursor += new Vector2(0, 10);
        int leftColumn = (int)cursor.X;
        int rightColumn = leftColumn + panelWidth + gutter;

        // Draw row 1
        DrawStat_HP(sb, new Rectangle(leftColumn, (int)cursor.Y, panelWidth, rowHeight), player);
        DrawStat_Mana(sb, new Rectangle(rightColumn, (int)cursor.Y, panelWidth, rowHeight), player);

        // Draw row 2
        int rowY2 = (int)cursor.Y + rowHeight + 6;
        DrawStat_Defense(sb, new Rectangle(leftColumn, rowY2, panelWidth, rowHeight), player);
        DrawStat_Attack(sb, new Rectangle(rightColumn, rowY2, panelWidth, rowHeight), player);

        // Draw row 3
        int rowY3 = (int)cursor.Y + rowHeight * 2 + 6 * 2;
        DrawStat_Coins(sb, new Rectangle(leftColumn, rowY3, panelWidth, rowHeight), player);
        DrawStat_Ammo(sb, new Rectangle(rightColumn, rowY3, panelWidth, rowHeight), player);

        // Draw row 4
        int rowY4 = (int)cursor.Y + rowHeight * 3 + 6 * 3;
        DrawStat_Minions(sb, new Rectangle(leftColumn, rowY4, panelWidth, rowHeight), player);
        DrawStat_Sentries(sb, new Rectangle(rightColumn, rowY4, panelWidth, rowHeight), player);

        // Draw row 5
        int rowY5 = (int)cursor.Y + rowHeight * 4 + 6 * 4;
        DrawStat_TimeInSession(sb, new Rectangle(leftColumn, rowY5, panelWidth, rowHeight), player);
        DrawStat_DaysInSession(sb, new Rectangle(rightColumn, rowY5, panelWidth, rowHeight), player);

        sb.End(); // restore
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                 DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
    }

    public static void DrawPlayer(SpriteBatch sb, Vector2 pos, Player player, float scale = 1.2f)
    {
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            Vector2 lockPos = pos + new Vector2(105, 65);
            MapHeadRendererHook.shouldFlipHeadDraw = player.direction == -1;
            Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, lockPos,
                1f, 1.5f, Color.White);
            MapHeadRendererHook.shouldFlipHeadDraw = false;
            DrawLock(sb, lockPos + new Vector2(-36, -15), 4f);
            return;
        }

        ModifyPlayerDrawInfo.ForceFullBrightOnce = true;
        try
        {
            // Draw player
            pos += Main.screenPosition + new Vector2(100, 90);

            // Celestial starboard (45) sucks
            if (player.wings == 45) player.wings = 0;

            Main.PlayerRenderer.DrawPlayer(Main.Camera, player, pos, player.fullRotation, player.fullRotationOrigin, 0f, scale);
        }
        finally
        {
            ModifyPlayerDrawInfo.ForceFullBrightOnce = false;
        }
    }
    public static void DrawMapFullscreenBackground(SpriteBatch sb, Rectangle rect, Player p)
    {
        var player = Main.LocalPlayer;
        if (player == null || !player.active) return;

        if (!HasAccess(Main.LocalPlayer, p))
        {
            // Draw surface
            var surfaceAsset = TextureAssets.MapBGs[0];
            sb.Draw(surfaceAsset.Value, rect, Color.YellowGreen * 0.5f);
            return;
        }

        var screenPos = Main.screenPosition;
        var tile = Main.tile[(int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)];
        if (tile == null) return;

        int wall = tile.wall;
        int bgIndex = -1;
        Color color = Color.White;

        if (screenPos.Y > (Main.maxTilesY - 232) * 16)
            bgIndex = 2;
        else if (player.ZoneDungeon)
            bgIndex = 4;
        else if (wall == 87)
            bgIndex = 13;
        else if (screenPos.Y > Main.worldSurface * 16.0)
        {
            bgIndex = wall switch
            {
                86 or 108 => 15,
                180 or 184 => 16,
                178 or 183 => 17,
                62 or 263 => 18,
                _ => player.ZoneGlowshroom ? 20 :
                     player.ZoneCorrupt ? player.ZoneDesert ? 39 : player.ZoneSnow ? 33 : 22 :
                     player.ZoneCrimson ? player.ZoneDesert ? 40 : player.ZoneSnow ? 34 : 23 :
                     player.ZoneHallow ? player.ZoneDesert ? 41 : player.ZoneSnow ? 35 : 21 :
                     player.ZoneSnow ? 3 :
                     player.ZoneJungle ? 12 :
                     player.ZoneDesert ? 14 :
                     player.ZoneRockLayerHeight ? 31 : 1
            };
        }
        else if (player.ZoneGlowshroom)
            bgIndex = 19;
        else
        {
            color = Main.ColorOfTheSkies;
            int midTileX = (int)((screenPos.X + Main.screenWidth / 2f) / 16f);

            if (player.ZoneSkyHeight) bgIndex = 32;
            else if (player.ZoneCorrupt) bgIndex = player.ZoneDesert ? 36 : 5;
            else if (player.ZoneCrimson) bgIndex = player.ZoneDesert ? 37 : 6;
            else if (player.ZoneHallow) bgIndex = player.ZoneDesert ? 38 : 7;
            else if (screenPos.Y / 16f < Main.worldSurface + 10.0 && (midTileX < 380 || midTileX > Main.maxTilesX - 380))
                bgIndex = 10;
            else if (player.ZoneSnow) bgIndex = 11;
            else if (player.ZoneJungle) bgIndex = 8;
            else if (player.ZoneDesert) bgIndex = 9;
            else if (Main.bloodMoon) { bgIndex = 25; color *= 2f; }
            else if (player.ZoneGraveyard) bgIndex = 26;
        }

        var asset = bgIndex >= 0 && bgIndex < TextureAssets.MapBGs.Length ? TextureAssets.MapBGs[bgIndex] : TextureAssets.MapBGs[0];

        sb.Draw(asset.Value, rect, color);
    }

    #region Stats
    public static void DrawStat_DaysInSession(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        // Draw sun or moondial
        Item icon = new(Main.dayTime ? ItemID.Sundial : ItemID.Moondial);
        Vector2 pos = new(rect.X + 16, rect.Y + 14);
        ItemSlot.DrawItemIcon(icon, 31, sb, pos, 1.0f, 32f, Color.White);

        // Draw days in session
        var text = SessionTrackerSystem.GetSessionDurationIngameDays(player.whoAmI);
        int xOffset = 0;
        if (text == string.Empty)
        {
            text = "n/a";
            xOffset = +3; // -55+3 = 52
        }
        var size = FontAssets.MouseText.Value.MeasureString(text);
        var textPos = new Vector2(rect.X + rect.Width - 52, rect.Y + 5);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, text, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }
    public static void DrawStat_TimeInSession(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        // Draw stopwatch
        Item icon = new(ItemID.Stopwatch);
        Vector2 pos = new(rect.X + 16, rect.Y + 14);
        ItemSlot.DrawItemIcon(icon, 31, sb, pos, 0.8f, 32f, Color.White);

        // Draw time in session
        var text = SessionTrackerSystem.GetSessionDuration(player.whoAmI);
        int xOffset = 0;
        if (text == string.Empty)
        {
            text = "n/a";
            xOffset = +3; // -55+3 = 52
        }

        if (text.Length > 6)
            xOffset = -5;

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        var textPos = new Vector2(rect.X + rect.Width - 55 + xOffset, rect.Y + 5);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, text, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }

    // Enemy animation state
    private static int _lastValidId = -1;
    private static int _enemyFrameCounter = 0;
    private static int _enemyFrameTimer = 0;

    // Boss animation state
    private static int _bossFrameCounter = 0;
    private static int _bossFrameTimer = 0;

    public static void DrawStat_LastBossHit(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        int npcId = -1; // TODO!
        if (npcId == -1)
        {
            // Draw question mark
            var questionTex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked");
            var rect2 = new Rectangle(rect.X, rect.Y, questionTex.Width(), questionTex.Height());
            sb.Draw(questionTex.Value, new Vector2(rect.X + 6, rect.Y + 1), null, Color.White, 0f,
                Vector2.Zero, scale: 0.8f, SpriteEffects.None, 0f);

            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "n/a",
                rect.X + 45, rect.Y + 4, Color.White, Color.Black, Vector2.Zero, 1f);
            return;
        }

        NPC npc = Main.npc[npcId];
        if (!npc.boss)
        {
            // Draw question mark
            var questionTex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked");
            var rect2 = new Rectangle(rect.X, rect.Y, questionTex.Width(), questionTex.Height());
            sb.Draw(questionTex.Value, new Vector2(rect.X + 6, rect.Y + 1), null, Color.White, 0f,
                Vector2.Zero, scale: 0.8f, SpriteEffects.None, 0f);

            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "n/a",
                rect.X + 45, rect.Y + 4, Color.White, Color.Black, Vector2.Zero, 1f);
            return;
        }

        Main.instance.LoadNPC(npc.type);
        Texture2D npcTex = TextureAssets.Npc[npc.type].Value;

        int frames = Main.npcFrameCount[npc.type];
        if (frames <= 0) frames = 1;

        int frameHeight = npcTex.Height / frames;
        Rectangle src = new(0, _bossFrameCounter * frameHeight, npcTex.Width, frameHeight);

        float scale = 36f / Math.Max(npcTex.Width, frameHeight);
        Vector2 pos = rect.Center.ToVector2() - new Vector2(npcTex.Width * scale + 75, frameHeight * scale) / 2f;

        _bossFrameTimer++;
        if (_bossFrameTimer > 14)
        {
            _bossFrameCounter = (_bossFrameCounter + 1) % frames;
            _bossFrameTimer = 0;
        }

        sb.Draw(npcTex, pos, src, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        string name = Lang.GetNPCNameValue(npc.type);
        if (name.Length > 8) name = name[..7] + "...";

        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, name,
            rect.X + 16, rect.Y + 4, Color.White, Color.Black, Vector2.Zero, 1f);
    }

    public static void DrawStat_LastEnemyHit(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        int bannerID = player.lastCreatureHit;
        int netID = bannerID > 0 ? Item.BannerToNPC(bannerID) : 0;

        // Only update if valid
        if (netID > 0 && netID < TextureAssets.Npc.Length && Main.npcFrameCount[netID] > 0)
        {
            _lastValidId = netID;
        }

        if (_lastValidId <= 0)
        {
            // Draw question mark
            var questionTex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked");
            var rect2 = new Rectangle(rect.X, rect.Y, questionTex.Width(), questionTex.Height());
            sb.Draw(questionTex.Value, new Vector2(rect.X + 6, rect.Y + 1), null, Color.White, 0f,
                Vector2.Zero, scale: 0.8f, SpriteEffects.None, 0f);

            Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, "n/a",
                rect.X + 45, rect.Y + 4, Color.White, Color.Black, Vector2.Zero, 1f);
            return;
        }

        Main.instance.LoadNPC(_lastValidId);
        Texture2D npcTex = TextureAssets.Npc[_lastValidId].Value;

        int frames = Main.npcFrameCount[_lastValidId];
        if (frames <= 0) frames = 1;

        int frameHeight = npcTex.Height / frames;
        Rectangle src = new Rectangle(0, _enemyFrameCounter * frameHeight, npcTex.Width, frameHeight);

        float scale = 36f / Math.Max(npcTex.Width, frameHeight);
        Vector2 pos = rect.Center.ToVector2() - new Vector2(npcTex.Width * scale + 75, frameHeight * scale) / 2f;

        _enemyFrameTimer++;
        if (_enemyFrameTimer > 14)
        {
            _enemyFrameCounter = (_enemyFrameCounter + 1) % frames;
            _enemyFrameTimer = 0;
        }

        sb.Draw(npcTex, pos, src, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        string name = Lang.GetNPCNameValue(_lastValidId);
        if (name.Length > 10) name = name[..9] + "..";
        var width = FontAssets.MouseText.Value.MeasureString(name).X;

        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, name,
            rect.X + tex.Width() - width - 1, rect.Y + 4, Color.White, Color.Black, Vector2.Zero, 1f);
    }

    public static void DrawStat_Minions(SpriteBatch sb, Rectangle rect, Player player)
    {
        var tex = Ass.StatPanel; rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height()); sb.Draw(tex.Value, rect, Color.White);
        int max = player.maxMinions; int cur = (int)Math.Round(player.slotsMinions); int bestProj = -1; float bestScore = 0f; var owned = player.ownedProjectileCounts; int limit = owned.Length;
        for (int t = 0; t < limit; t++)
        {
            int c = owned[t]; if (c <= 0) continue; var proj = ContentSamples.ProjectilesByType[t]; if (!proj.minion) continue; if (!(proj.minionSlots > 0f)) continue; float score = c * proj.minionSlots; if (score > bestScore) { bestScore = score; bestProj = t; }
        }
        Item icon = new(ItemID.SlimeStaff);
        if (bestProj >= 0)
        {
            foreach (var kv in ContentSamples.ItemsByType)
            {
                var it = kv.Value; if (it == null) continue; if (it.summon || it.DamageType == DamageClass.Summon) { if (it.shoot == bestProj) { icon = new Item(it.type); break; } }
            }
        }
        var pos = new Vector2(rect.X + 15, rect.Y + 15);
        ItemSlot.DrawItemIcon(icon, 31, sb, pos, 0.9f, 32f, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        var tp = new Vector2(rect.X + 45, rect.Y + 4);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, $"{cur}/{max}", tp.X, tp.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }
    public static void DrawStat_Sentries(SpriteBatch sb, Rectangle rect, Player player)
    {
        var tex = Ass.StatPanel; 
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height()); 
        sb.Draw(tex.Value, rect, Color.White);
        int max = player.maxTurrets; int cur = 0; int bestProj = -1; int bestCount = 0;
        for (int t = 0; t < player.ownedProjectileCounts.Length; t++)
        {
            int c = player.ownedProjectileCounts[t]; if (c <= 0) continue;
            var proj = ContentSamples.ProjectilesByType[t];
            if (!proj.sentry) continue;
            cur += c;
            if (c > bestCount) { bestCount = c; bestProj = t; }
        }
        Item icon = new(ItemID.QueenSpiderStaff); // fallback
        if (bestProj >= 0)
        {
            foreach (var kv in ContentSamples.ItemsByType)
            {
                var it = kv.Value; if (it == null) continue;
                if (it.summon || it.DamageType == DamageClass.Summon) { if (it.shoot == bestProj) { icon = new Item(it.type); break; } }
            }
        }
        var pos = new Vector2(rect.X + 15, rect.Y + 15);
        ItemSlot.DrawItemIcon(icon, 31, sb, pos, 0.9f, 32f, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        var tp = new Vector2(rect.X + 45, rect.Y + 4);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, $"{cur}/{max}", tp.X, tp.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }
    public static void DrawStat_Defense(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Defense");
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        // Draw defense NPCName
        var defenseText = $"{player.statDefense}";
        var snippets = ChatManager.ParseMessage(defenseText, Color.White).ToArray();
        var pos = new Vector2(rect.X + 45, rect.Y + 4);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, Vector2.One, out _);
    }

    public static void DrawStat_HP(SpriteBatch sb, Rectangle rect, Player player)
    {
        var tex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_HP");
        sb.Draw(tex.Value, rect, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        string text = $"{player.statLife}/{player.statLifeMax2}";
        ChatManager.DrawColorCodedStringWithShadow(
            sb, FontAssets.MouseText.Value,
            ChatManager.ParseMessage(text, Color.White).ToArray(),
            new Vector2(rect.X + 32, rect.Y + 4),
            0f, Vector2.Zero, Vector2.One, out _);
    }

    public static void DrawStat_Mana(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        // Draw mana texture
        var manaTex = TextureAssets.Mana;
        var manaRect = new Rectangle(rect.X + 4, rect.Y + 2, manaTex.Width(), manaTex.Height());
        sb.Draw(TextureAssets.Mana.Value, manaRect, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        // Draw mana NPCName
        var manaText = $"{player.statMana}/{player.statManaMax2}";
        var size = FontAssets.MouseText.Value.MeasureString(manaText);
        var textPos = new Vector2(rect.X + rect.Width - size.X - 5, rect.Y + 5);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, manaText, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }

    public static void DrawStat_Attack(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Attack");
        var rect2 = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect2, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        // Get damage
        var item = player.HeldItem;
        if (item == null || item.IsAir) return;
        var dmgType = item.DamageType;
        int baseDamage = item.damage;
        int effectiveDamage = (int)player.GetTotalDamage(dmgType).ApplyTo(baseDamage);
        string damageText = effectiveDamage.ToString();
        if (baseDamage <= 0)
        {
            damageText = "n/a";
        }

        // Draw damage text
        var size = FontAssets.MouseText.Value.MeasureString(damageText);
        var textPos = new Vector2(rect.X + 45, rect.Y + 4);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, damageText, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }

    public static void DrawStat_Coins(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background stat panel
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            Vector2 goldCoinPos = new(rect.X + 17, rect.Y + 15);
            ItemSlot.DrawItemIcon(new Item(ItemID.GoldCoin), 31, sb, goldCoinPos, 1f, 32, Color.White);
            Vector2 lockPos = new(rect.X + 40, rect.Y);
            DrawLock(sb, lockPos);
            return;
        }

        // Count total
        long CountCoins(Item[] items, params int[] excludeSlots)
        {
            if (items == null) return 0;
            long sum = 0;
            bool Excluded(int i) { foreach (var e in excludeSlots) if (i == e) return true; return false; }
            for (int i = 0; i < items.Length; i++)
            {
                if (Excluded(i)) continue;
                var it = items[i];
                if (it == null || it.IsAir) continue;
                switch (it.type)
                {
                    case ItemID.PlatinumCoin: sum += it.stack * 1_000_000L; break;
                    case ItemID.GoldCoin: sum += it.stack * 10_000L; break;
                    case ItemID.SilverCoin: sum += it.stack * 100L; break;
                    case ItemID.CopperCoin: sum += it.stack; break;
                }
            }
            return sum;
        }

        long total =
            CountCoins(player.inventory, 58, 57, 56, 55, 54) +
            CountCoins(player.bank?.item) +
            CountCoins(player.bank2?.item) +
            CountCoins(player.bank3?.item) +
            CountCoins(player.bank4?.item);

        int[] coins = Utils.CoinsSplit(total);
        int c = coins[0], s = coins[1], g = coins[2], p = coins[3];

        var font = FontAssets.MouseText.Value;
        var pos = new Vector2(rect.X + 6, rect.Y + 8);
        int iconSize = 32;
        float scale = 0.8f;
        float dx = 28f;

        if (p <= 99)
        {
            // Draw total for poor player
            ItemSlot.DrawItemIcon(new Item(ItemID.PlatinumCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, p.ToString(), pos.X - 5, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
            pos.X += dx;
            ItemSlot.DrawItemIcon(new Item(ItemID.GoldCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, g.ToString(), pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
            pos.X += dx;
            ItemSlot.DrawItemIcon(new Item(ItemID.SilverCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, s.ToString(), pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
            pos.X += dx;
            ItemSlot.DrawItemIcon(new Item(ItemID.CopperCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, c.ToString(), pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
        }
        else
        {
            // Draw total for rich player
            pos.X += 4; scale = 0.8f;
            ItemSlot.DrawItemIcon(new Item(ItemID.PlatinumCoin), 31, sb, pos + new Vector2(11, 0), scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, p.ToString(), pos.X - 5, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
            pos.X += dx + 14;
            ItemSlot.DrawItemIcon(new Item(ItemID.GoldCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, g.ToString(), pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
            pos.X += dx;
            ItemSlot.DrawItemIcon(new Item(ItemID.SilverCoin), 31, sb, pos, scale, iconSize, Color.White);
            Utils.DrawBorderStringFourWay(sb, font, s.ToString(), pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, scale);
        }
    }

    public static void DrawStat_Ammo(SpriteBatch sb, Rectangle rect, Player player)
    {
        // Draw background
        var tex = Ass.StatPanel;
        rect = new Rectangle(rect.X, rect.Y, tex.Width(), tex.Height());
        sb.Draw(tex.Value, rect, Color.White);

        // Summarize stacks for each ammo slot and draw the one with the most
        int highestStack = 0;
        Item highestStackItem = new(ItemID.WoodenArrow);
        for (int i = 54; i <= 57; i++)
        {
            var item = player.inventory[i];
            if (item.IsAir) continue;

            if (item.stack > highestStack)
            {
                highestStack = item.stack;
                highestStackItem = item;
            }
        }

        // Draw item
        var pos = new Vector2(rect.X + 15, rect.Y + 15);
        ItemSlot.DrawItemIcon(highestStackItem, 31, sb, pos, 0.8f, 32f, Color.White);

        if (!HasAccess(Main.LocalPlayer, player))
        {
            DrawLock(sb, new Vector2(rect.X + 40, rect.Y));
            return;
        }

        // Draw ammo count
        string highestAmmoStack = highestStack.ToString();
        var size = FontAssets.MouseText.Value.MeasureString(highestAmmoStack);
        var textPos = new Vector2(rect.X + 45, rect.Y + 4);
        Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, highestAmmoStack, textPos.X, textPos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
    }
    #endregion Stats

    #region Info Texts
    private static void DrawHeaderText(SpriteBatch sb, Rectangle rect, Player player)
    {
        string playerText = $"Player: {player.name}";

        var snippets = ChatManager.ParseMessage(playerText, Color.White).ToArray();
        Vector2 textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, playerText, Vector2.One);
        float textWidth = textSize.X;
        Vector2 pos = new(rect.X + (rect.Width - textWidth) / 2f, rect.Y + 6);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, Vector2.One, out _);
    }
    public static void DrawTeamText(SpriteBatch sb, Vector2 pos, Player player)
    {
        string teamText = player.team switch
        {
            0 => "No team",
            1 => "[c/DA3B3B:Team Red]",
            2 => "[c/3bda55:Team Green]",
            3 => "[c/3b95da:Team Blue]",
            4 => "[c/f2dd64:Team Yellow]",
            5 => "[c/e064f2:Team Pink]",
            _ => string.Empty
        };

        var snippets = ChatManager.ParseMessage(teamText, Color.White).ToArray();
        pos += new Vector2(7, 5);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, new Vector2(1.0f), out _);
    }

    public static void DrawPlayerIDText(SpriteBatch sb, Vector2 pos, Player player)
    {
        string ID = "ID: " + player.whoAmI.ToString();

        var snippets = ChatManager.ParseMessage(ID, Color.White).ToArray();
        pos += new Vector2(7, 5);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, new Vector2(1.0f), out _);
    }
    #endregion

    #region Helpers
    public static bool HasAccess(Player viewer, Player target)
    {
        if (viewer == target || viewer == Main.LocalPlayer && target == Main.LocalPlayer)
            return true;

        //if (Main.netMode == NetmodeID.SinglePlayer)
        //{
        //    var sp_privacy = PrivacyCache.Get(target.whoAmI);
        //    return sp_privacy switch
        //    {
        //        Config.Privacy.Everyone => true,
        //        Config.Privacy.Team => true,
        //        _ => viewer == target // NoOne: only yourself
        //    };
        //}

        var privacy = PrivacyCache.Get(target.whoAmI);
        return privacy switch
        {
            Config.Privacy.Everyone => true,
            Config.Privacy.Team =>
                viewer != null && target != null &&
                viewer.team != 0 && viewer.team == target.team,
            _ => viewer == target // NoOne: only yourself
        };
    }

    #endregion

    #region Panels & Separators
    public static void DrawSeparatorBorder(SpriteBatch sb, Rectangle rect, int edgeWidth = 2)
    {
        var tex = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;
        //var tex = TextureAssets.MagicPixel.Value;
        Color color = new Color(89, 116, 213, 255) * 1.0f;

        DrawPanel(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Y), rect.Width, color);
        DrawPanel(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Bottom - edgeWidth), rect.Width, color);
        DrawPanelVertical(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Y), rect.Height, color);
        DrawPanelVertical(tex, edgeWidth, 0, sb, new Vector2(rect.Right - edgeWidth, rect.Y), rect.Height, color);
    }

    public static void DrawPanel(Texture2D texture, int edgeWidth, int edgeShove, SpriteBatch spriteBatch, Vector2 position, float width, Color color)
    {
        spriteBatch.Draw(texture,
            new Vector2(position.X + edgeWidth, position.Y), new Rectangle(edgeWidth + edgeShove, 0, texture.Width - (edgeWidth + edgeShove) * 2, texture.Height),
            color,
            0f,
            Vector2.Zero,
            new Vector2((width - edgeWidth * 2) / (texture.Width - (edgeWidth + edgeShove) * 2), 1f),
            SpriteEffects.None,
            0f);
    }

    public static void DrawPanelVertical(Texture2D texture, int edgeWidth, int edgeShove, SpriteBatch spriteBatch, Vector2 position, float height, Color color)
    {
        spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y + edgeWidth, edgeWidth, (int)height - edgeWidth * 2),
            new Rectangle(texture.Width / 2, 0, 1, texture.Height), color);
    }

    public static void DrawFullBGPanel(SpriteBatch sb, Rectangle rect)
    {
        var BG = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        var Border = Main.Assets.Request<Texture2D>("Images/UI/PanelBorder");

        const int corner = 12;
        const int bar = 4;

        static void DrawNineSlice(SpriteBatch s, Texture2D tex, Rectangle dst, Color col)
        {
            int w = Math.Max(dst.Width, corner * 2 + 1);
            int h = Math.Max(dst.Height, corner * 2 + 1);
            int x0 = dst.X, y0 = dst.Y;
            int x1 = x0 + corner, y1 = y0 + corner;
            int x2 = x0 + w - corner, y2 = y0 + h - corner;
            s.Draw(tex, new Rectangle(x0, y0, corner, corner), new Rectangle(0, 0, corner, corner), col);
            s.Draw(tex, new Rectangle(x2, y0, corner, corner), new Rectangle(corner + bar, 0, corner, corner), col);
            s.Draw(tex, new Rectangle(x0, y2, corner, corner), new Rectangle(0, corner + bar, corner, corner), col);
            s.Draw(tex, new Rectangle(x2, y2, corner, corner), new Rectangle(corner + bar, corner + bar, corner, corner), col);
            s.Draw(tex, new Rectangle(x1, y0, w - corner * 2, corner), new Rectangle(corner, 0, bar, corner), col);
            s.Draw(tex, new Rectangle(x1, y2, w - corner * 2, corner), new Rectangle(corner, corner + bar, bar, corner), col);
            s.Draw(tex, new Rectangle(x0, y1, corner, h - corner * 2), new Rectangle(0, corner, corner, bar), col);
            s.Draw(tex, new Rectangle(x2, y1, corner, h - corner * 2), new Rectangle(corner + bar, corner, corner, bar), col);
            s.Draw(tex, new Rectangle(x1, y1, w - corner * 2, h - corner * 2), new Rectangle(corner, corner, bar, bar), col);
        }

        DrawNineSlice(sb, BG.Value, rect, new Color(63, 82, 151) * 1.0f);
        DrawNineSlice(sb, Border.Value, rect, Color.Black);
    }

    private static void DrawLock(SpriteBatch sb, Vector2 pos, float scale = 1.5f)
    {
        Texture2D tex = TextureAssets.HbLock[0].Value;

        // Calculate the width of one frame (assuming horizontal split)
        int frameWidth = tex.Width / 2;
        Rectangle sourceRect = new(0, 0, frameWidth, tex.Height);

        sb.Draw(tex, pos, sourceRect, Color.White, 0f, Vector2.Zero,
            scale: scale, SpriteEffects.None, 0f);
    }

    public static void DrawHorizontalSeparator(SpriteBatch sb, Vector2 pos, float width, int edgeWidth = 2)
    {
        var tex = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;
        Color color = new Color(89, 116, 213, 255) * 0.9f;
        DrawPanel(tex, edgeWidth, 0, sb, pos, width, color);
    }
    #endregion
    private static void DrawDebugRect(Rectangle r) => Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, r, Color.Red * 0.5f);
}
