﻿using BaseCAD.Geometry;
using BaseCAD.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Drawables
{
    public class Point : Drawable
    {
        private Point2D p;

        public Point2D Location { get => p; set { p = value; NotifyPropertyChanged(); } }

        [Browsable(false)]
        public float X { get { return Location.X; } }
        [Browsable(false)]
        public float Y { get { return Location.Y; } }

        public Point() { }

        public Point(Point2D location)
        {
            Location = location;
        }

        public Point(float x, float y)
            : this(new Point2D(x, y))
        {
            ;
        }

        public override void Draw(Renderer renderer)
        {
            float size = renderer.View.ScreenToWorld(new Vector2D(renderer.View.Document.Settings.Get<int>("PointSize"), 0)).X / 2;
            renderer.DrawCircle(Style.ApplyLayer(Layer), Location, size);
        }

        public override Extents2D GetExtents()
        {
            Extents2D extents = new Extents2D();
            extents.Add(X, Y);
            return extents;
        }

        public override void TransformBy(Matrix2D transformation)
        {
            Location = Location.Transform(transformation);
        }

        public override bool Contains(Point2D pt, float pickBoxSize)
        {
            float dist = (pt - Location).Length;
            return dist <= pickBoxSize / 2;
        }

        public override ControlPoint[] GetControlPoints()
        {
            return new[]
            {
                new ControlPoint("Location", Location),
            };
        }

        public override void TransformControlPoint(int index, Matrix2D transformation)
        {
            if (index == 0)
                Location = Location.Transform(transformation);
        }

        public override void Load(DocumentReader reader)
        {
            base.Load(reader);
            Location = reader.ReadPoint2D();
        }

        public override void Save(DocumentWriter writer)
        {
            base.Save(writer);
            writer.Write(Location);
        }
    }
}
