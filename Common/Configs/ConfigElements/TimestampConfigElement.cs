using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;

namespace ChatPlus.Common.Configs.ConfigElements;

// Used for timestamp.
public class TimestampConfigElement<TEnum> : RangeElement where TEnum : struct, Enum
{
    private TEnum[] values;

    public override int NumberTicks => values.Length;

    public override float TickIncrement
    {
        get
        {
            int denom = values.Length - 1;
            if (denom <= 0) denom = 1;
            return 1f / denom;
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);

        // Position
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        Rectangle rect = new((int)pos.X+175, (int)pos.Y, 30, 30);

        //var current = values[GetIndex()];

        //if (current.ToString().Contains("Grid"))
        //{
        //    sb.Draw(Ass.FilterGrid.Value, rect, Color.White);
        //}
        //else if (current.ToString().Contains("List"))
        //{
        //    sb.Draw(Ass.FilterList.Value, rect, Color.White);
        //}
    }

    protected override float Proportion
    {
        get
        {
            int denom = values.Length - 1;
            if (denom <= 0) denom = 1;
            return (float)GetIndex() / denom;
        }
        set
        {
            int denom = values.Length - 1;
            if (denom <= 0) denom = 1;
            int index = (int)Math.Round(value * denom);
            SetIndex(index);
        }
    }

    public override void OnBind()
    {
        base.OnBind();

        values = (TEnum[])Enum.GetValues(typeof(TEnum));

        TextDisplayFunction = () =>
        {
            string label = Label;
            if (string.IsNullOrEmpty(label)) label = MemberInfo.Name;

            int i = GetIndex();
            if (i < 0) i = 0;
            if (i >= values.Length) i = values.Length - 1;

            if (label.Length < 20)
            {
                return label + ": " + GetLiveLabel(values[i]);
            }
            return label + ": " + GetLiveLabel(values[i]);
        };
    }

    private int GetIndex()
    {
        object raw = MemberInfo.GetValue(Item);
        if (raw is TEnum current)
        {
            int idx = Array.IndexOf(values, current);
            if (idx >= 0) return idx;
        }
        return 0;
    }

    private void SetIndex(int index)
    {
        if (index < 0) index = 0;
        if (index >= values.Length) index = values.Length - 1;

        if (MemberInfo.CanWrite)
        {
            MemberInfo.SetValue(Item, values[index]);
            Interface.modConfig.SetPendingChanges();
        }
    }

    private string GetLiveLabel(TEnum value)
    {
        if (typeof(TEnum) == typeof(ClientConfig.TimestampSettings))
        {
            var v = (ClientConfig.TimestampSettings)(object)value;
            if (v == ClientConfig.TimestampSettings.Off) return "Off";

            string fmt = "[" + GetFormat(v) + "]";
            return DateTime.Now.ToString(fmt);
        }

        return value.ToString();
    }

    private static string GetFormat(ClientConfig.TimestampSettings value)
    {
        if (value == ClientConfig.TimestampSettings.HourAndMinute12Hours)
        {
            return "h:mm tt";
        }
        if (value == ClientConfig.TimestampSettings.HourAndMinuteAndSeconds12Hours)
        {
            return "h:mm:ss tt";
        }
        if (value == ClientConfig.TimestampSettings.HourAndMinute24Hours)
        {
            return "HH:mm";
        }
        if (value == ClientConfig.TimestampSettings.HourAndMinuteAndSeconds24Hours)
        {
            return "HH:mm:ss";
        }
        return "HH:mm";
    }
}
