﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    public partial class Editor
    {
        internal enum InputMode
        {
            None,
            Point,
            Angle,
            Text
        }

        public class InputResult
        {
            public bool Success { get; private set; }

            public InputResult(bool success)
            {
                Success = success;
            }
        }

        public class PointResult : InputResult
        {
            public Point2D Location { get; private set; }

            public PointResult(bool success, Point2D location) : base(success)
            {
                Location = location;
            }
        }
        public class AngleResult : InputResult
        {
            public Vector2D Direction { get; private set; }

            public AngleResult(bool success, Vector2D direction) : base(success)
            {
                Direction = direction;
            }
        }

        public class TextResult : InputResult
        {
            public string Text { get; private set; }

            public TextResult(bool success, string text) : base(success)
            {
                Text = text;
            }
        }

        public class InputOptions<T>
        {
            public string Message { get; private set; }
            public Action<T> Jig { get; private set; }

            public InputOptions(string message)
            {
                Message = message;
                Jig = (p) => { };
            }

            public InputOptions(string message, Action<T> jig)
            {
                Message = message;
                Jig = jig;
            }
        }

        public class PointOptions : InputOptions<Point2D>
        {
            public bool HasBasePoint { get; private set; }
            public Point2D BasePoint { get; private set; }
            public PointOptions(string message, Point2D basePoint, Action<Point2D> jig) : base(message, jig)
            {
                HasBasePoint = true;
                BasePoint = basePoint;
            }

            public PointOptions(string message, Point2D basePoint) : base(message)
            {
                HasBasePoint = true;
                BasePoint = basePoint;
            }
            public PointOptions(string message, Action<Point2D> jig) : base(message, jig)
            {
                HasBasePoint = false;
                BasePoint = Point2D.Zero;
            }

            public PointOptions(string message) : base(message)
            {
                HasBasePoint = false;
                BasePoint = Point2D.Zero;
            }
        }

        public class AngleOptions : InputOptions<Vector2D>
        {
            public Point2D BasePoint { get; private set; }
            public AngleOptions(string message, Point2D basePoint, Action<Vector2D> jig) : base(message, jig)
            {
                BasePoint = basePoint;
            }

            public AngleOptions(string message, Point2D basePoint) : base(message)
            {
                BasePoint = basePoint;
            }
        }
        public class TextOptions : InputOptions<string>
        {
            public TextOptions(string message, Action<string> jig) : base(message, jig)
            {
                ;
            }

            public TextOptions(string message) : base(message)
            {
                ;
            }
        }
    }
}
