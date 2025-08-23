﻿using BaseCAD;
using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseCAD
{
    internal enum InputMode
    {
        None,
        Selection,
        Point,
        Angle,
        Text,
        Distance,
        Corner,
        Int,
        Float
    }

    public enum ResultMode
    {
        OK,
        Keyword,
        Cancel
    }

    public abstract class InputResult<T>
    {
        public ResultMode Result { get; private set; }
        public T Value { get; private set; }
        public string Keyword { get; private set; }

        protected InputResult(ResultMode result, T value, string keyword)
        {
            Result = result;
            Value = value;
            Keyword = keyword;
        }
    }

    public class FilenameResult : InputResult<string>
    {
        internal FilenameResult(ResultMode result) : base(result, "", "") { }
        internal FilenameResult(string value) : base(ResultMode.OK, value, "") { }
    }

    public class SaveFilenameResult : InputResult<string>
    {
        internal SaveFilenameResult(ResultMode result) : base(result, "", "") { }
        internal SaveFilenameResult(string value) : base(ResultMode.OK, value, "") { }
    }

    public class SelectionResult : InputResult<SelectionSet>
    {
        internal SelectionResult(ResultMode result) : base(result, new SelectionSet(), "") { }
        internal SelectionResult(SelectionSet value) : base(ResultMode.OK, value, "") { }
        internal SelectionResult(string keyword) : base(ResultMode.Keyword, new SelectionSet(), keyword) { }
    }

    public class PointResult : InputResult<Point2D>
    {
        internal PointResult(ResultMode result) : base(result, Point2D.Zero, "") { }
        internal PointResult(Point2D value) : base(ResultMode.OK, value, "") { }
        internal PointResult(string keyword) : base(ResultMode.Keyword, Point2D.Zero, keyword) { }
    }

    public class AngleResult : InputResult<float>
    {
        internal AngleResult(ResultMode result) : base(result, 0, "") { }
        internal AngleResult(float value) : base(ResultMode.OK, value, "") { }
        internal AngleResult(string keyword) : base(ResultMode.Keyword, 0, keyword) { }
    }

    public class TextResult : InputResult<string>
    {
        internal TextResult(ResultMode result) : base(result, "", "") { }
        internal TextResult(string value) : base(ResultMode.OK, value, "") { }
    }
    public class IntResult : InputResult<int>
    {
        internal IntResult(ResultMode result) : base(result, 0, "") { }
        internal IntResult(int value) : base(ResultMode.OK, value, "") { }
        internal IntResult(string keyword) : base(ResultMode.Keyword, 0, keyword) { }
    }
    public class FloatResult : InputResult<float>
    {
        internal FloatResult(ResultMode result) : base(result, 0, "") { }
        internal FloatResult(float value) : base(ResultMode.OK, value, "") { }
        internal FloatResult(string keyword) : base(ResultMode.Keyword, 0, keyword) { }
    }
    public class DistanceResult : InputResult<float>
    {
        public Point2D Point { get; internal set; }

        internal DistanceResult(ResultMode result) : base(result, 0, "") { }
        internal DistanceResult(float value, Point2D point) : base(ResultMode.OK, value, "") { Point = point; }
        internal DistanceResult(string keyword) : base(ResultMode.Keyword, 0, keyword) { }
    }

    public abstract class InputOptions
    {
        private static Regex upperOnly = new Regex("[^A-Z]", RegexOptions.Compiled);
        public string Message { get; set; }
        internal List<string> Keywords { get; private set; }
        internal List<string> Aliases { get; private set; }
        internal string DefaultKeyword { get; private set; }

        public InputOptions(string message)
        {
            Message = message;
            Keywords = new List<string>();
            Aliases = new List<string>();
            DefaultKeyword = "";
        }

        public void AddKeyword(string keyword, bool isDefault = false)
        {
            Keywords.Add(keyword);
            string alias = upperOnly.Replace(keyword, "");
            Aliases.Add(alias);

            if (isDefault) SetDefaultKeyword(keyword);
        }

        internal string MatchKeyword(string input)
        {
            if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(DefaultKeyword))
                return DefaultKeyword;
            if (string.IsNullOrEmpty(input))
                return "";

            for (int i = 0; i < Aliases.Count; i++)
            {
                if (string.Compare(Aliases[i], input, StringComparison.OrdinalIgnoreCase) == 0)
                    return Keywords[i];
            }
            for (int i = 0; i < Keywords.Count; i++)
            {
                if (Keywords[i].StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    return Keywords[i];
            }
            return "";
        }

        internal void SetDefaultKeyword(string keyword)
        {
            DefaultKeyword = keyword;
        }

        internal virtual string GetFullPrompt()
        {
            if (Keywords.Count == 0)
            {
                return Message.TrimEnd(' ', ':') + ": ";
            }
            else
            {
                StringBuilder sb = new StringBuilder(Message.TrimEnd(' ', ':'));
                sb.Append(" [");
                sb.Append(string.Join(", ", Keywords));
                sb.Append("]");
                if (Keywords.Contains(DefaultKeyword))
                {
                    sb.Append(" <");
                    sb.Append(DefaultKeyword);
                    sb.Append(">");
                }
                sb.Append(": ");
                return sb.ToString();
            }
        }
    }

    public class JigOptions<T> : InputOptions
    {
        public Action<T> Jig { get; private set; }

        public JigOptions(string message) : this(message, (p) => { })
        {
            ;
        }

        public JigOptions(string message, Action<T> jig) : base(message)
        {
            Jig = jig;
        }
    }

    public class FilenameOptions : InputOptions
    {
        public string FileName { get; set; }
        public string Filter { get; set; }
        public string Extension { get; set; }

        public FilenameOptions(string message, string filename, string filter, string ext) : base(message)
        {
            FileName = filename;
            Filter = filter;
            Extension = ext;
        }

        public FilenameOptions(string message, string filename, string filter)
            : this(message, filename, filter, "scf")
        {
            ;
        }

        public FilenameOptions(string message, string filename)
            : this(message, filename, "SimpleCAD file (*.scf)|*.scf|All files (*.*)|*.*", "scf")
        {
            ;
        }

        public FilenameOptions(string message)
            : this(message, "", "SimpleCAD file (*.scf)|*.scf|All files (*.*)|*.*", "scf")
        {
            ;
        }
    }

    public class SelectionOptions : InputOptions
    {
        public SelectionOptions(string message) : base(message)
        {
            ;
        }
    }

    public class PointOptions : JigOptions<Point2D>
    {
        public bool HasBasePoint { get; private set; }
        public Point2D BasePoint { get; private set; }

        public PointOptions(string message, Point2D basePoint, Action<Point2D> jig) : base(message, jig)
        {
            HasBasePoint = true;
            BasePoint = basePoint;
        }

        public PointOptions(string message, Point2D basePoint) : this(message, basePoint, (p) => { })
        {
            ;
        }

        public PointOptions(string message, Action<Point2D> jig) : base(message, jig)
        {
            HasBasePoint = false;
            BasePoint = Point2D.Zero;
        }

        public PointOptions(string message) : this(message, (p) => { })
        {
            ;
        }
    }
    public class CornerOptions : JigOptions<Point2D>
    {
        public Point2D BasePoint { get; private set; }

        public CornerOptions(string message, Point2D basePoint, Action<Point2D> jig) : base(message, jig)
        {
            BasePoint = basePoint;
        }

        public CornerOptions(string message, Point2D basePoint) : this(message, basePoint, (p) => { })
        {
            ;
        }
    }
    public class AngleOptions : JigOptions<float>
    {
        public Point2D BasePoint { get; set; }
        public AngleOptions(string message, Point2D basePoint, Action<float> jig) : base(message, jig)
        {
            BasePoint = basePoint;
        }

        public AngleOptions(string message, Point2D basePoint) : this(message, basePoint, (p) => { })
        {
            ;
        }
    }

    public class DistanceOptions : JigOptions<float>
    {
        public Point2D BasePoint { get; set; }

        public DistanceOptions(string message, Point2D basePoint, Action<float> jig) : base(message, jig)
        {
            BasePoint = basePoint;
        }

        public DistanceOptions(string message, Point2D basePoint) : this(message, basePoint, (p) => { })
        {
            ;
        }
    }

    public class TextOptions : JigOptions<string>
    {
        public TextOptions(string message, Action<string> jig) : base(message, jig)
        {
            ;
        }

        public TextOptions(string message) : this(message, (p) => { })
        {
            ;
        }

        internal override string GetFullPrompt()
        {
            return Message.TrimEnd(' ', ':') + ": ";
        }
    }
    public class NumberOptions<T> : JigOptions<T>
    {
        public bool AllowZero { get; set; } = true;
        public bool AllowNegative { get; set; } = true;
        public bool AllowPositive { get; set; } = true;

        public NumberOptions(string message, Action<T> jig) : base(message, jig)
        {
            ;
        }

        public NumberOptions(string message) : this(message, (p) => { })
        {
            ;
        }
    }
    public class IntOptions : NumberOptions<int>
    {
        public IntOptions(string message, Action<int> jig) : base(message, jig)
        {
            ;
        }

        public IntOptions(string message) : this(message, (p) => { })
        {
            ;
        }
    }

    public class FloatOptions : NumberOptions<float>
    {
        public FloatOptions(string message, Action<float> jig) : base(message, jig)
        {
            ;
        }

        public FloatOptions(string message) : this(message, (p) => { })
        {
            ;
        }
    }
}