﻿using BaseCAD.Geometry;
using System;
using System.ComponentModel;
using System.IO;

namespace BaseCAD.Drawables
{
    public class Rectangle : Drawable
    {
        private Point2D center;
        private float width;
        private float height;
        private float rotation;
        public Point2D Center { get { return center; } set { center = value; UpdatePolyline(); NotifyPropertyChanged(); } }
        public float Width { get { return width; } set { width = Math.Abs(value); UpdatePolyline(); NotifyPropertyChanged(); } }
        public float Height { get { return height; } set { height = Math.Abs(value); UpdatePolyline(); NotifyPropertyChanged(); } }
        public Point2D Corner
        {
            get
            {
                return Center + new Vector2D(Width / 2, Height / 2).Transform(Matrix2D.Rotation(Rotation)); ;
            }
            set
            {
                Vector2D size = (value - center).Transform(Matrix2D.Rotation(-Rotation));
                width = size.X * 2;
                height = size.Y * 2;
                UpdatePolyline();
                NotifyPropertyChanged();
            }
        }
        public float Rotation { get { return rotation; } set { rotation = value; UpdatePolyline(); NotifyPropertyChanged(); } }

        [Browsable(false)]
        public float X { get { return Center.X; } }
        [Browsable(false)]
        public float Y { get { return Center.Y; } }

        private Polyline poly;

        public Rectangle(Point2D center, float width, float height, float rotation = 0)
        {
            Center = center;
            Width = width;
            Height = height;
            Rotation = rotation;
            UpdatePolyline();
        }

        public Rectangle(float x, float y, float width, float height, float rotation = 0)
            : this(new Point2D(x, y), width, height, rotation)
        {
            ;
        }
        public Rectangle(Point2D center, Point2D corner, float rotation = 0)
    : this(center, (corner - center).X * 2, (corner - center).Y * 2, rotation)
        {
            ;
        }

        private void UpdatePolyline()
        {
            poly = new Polyline();
            poly.Points.Add(-Width / 2, -Height / 2);
            poly.Points.Add(+Width / 2, -Height / 2);
            poly.Points.Add(+Width / 2, +Height / 2);
            poly.Points.Add(-Width / 2, +Height / 2);
            poly.Closed = true;
            poly.TransformBy(Matrix2D.Rotation(rotation));
            poly.TransformBy(Matrix2D.Translation(X, Y));

        }

        public override void Draw(Renderer renderer)
        {
            poly.Style = Style;
            renderer.Draw(poly);
        }

        public override Extents2D GetExtents()
        {
            return poly.GetExtents();
        }

        public override void TransformBy(Matrix2D transformation)
        {
            Center = Center.Transform(transformation);
            Rotation = Vector2D.FromAngle(Rotation).Transform(transformation).Angle;
            UpdatePolyline();
        }
        public override bool Contains(Point2D pt, float pickBoxSize)
        {
            return poly.Contains(pt, pickBoxSize);
        }
        public override ControlPoint[] GetControlPoints()
        {
            return new[]
            {
                new ControlPoint("Center", Center),
                new ControlPoint("Corner", Corner),
                new ControlPoint("Rotation", ControlPoint.ControlPointType.Angle, Center, Center + Vector2D.FromAngle(Rotation) * Width / 2),
            };
        }
        public override void TransformControlPoint(int index, Matrix2D transformation)
        {
            if (index == 0)
                Center = Center.Transform(transformation);
            else if (index == 1)
                Corner = Corner.Transform(transformation);
            else if (index == 2)
                Rotation = Vector2D.FromAngle(Rotation).Transform(transformation).Angle;
        }
        public Rectangle(BinaryReader reader) : base(reader)
        {
            Center = new Point2D(reader);
            Rotation = reader.ReadSingle();
            Corner = new Point2D(reader);
            UpdatePolyline();
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            Center.Save(writer);
            writer.Write(Rotation);
            Corner.Save(writer);
        }
    }
}
