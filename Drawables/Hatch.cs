﻿using BaseCAD.Geometry;
using BaseCAD.Graphics;
using System.ComponentModel;

namespace BaseCAD.Drawables
{
    public class Hatch : Polyline
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Closed { get => true; set => throw new InvalidOperationException("Hatch must be a closed area."); }

        public Hatch() : base()
        {
            ;
        }

        public Hatch(Point2DCollection pts) : base(pts)
        {
            ;
        }

        public Hatch(params Point2D[] pts) : base(pts)
        {
            ;
        }

        public Hatch(PointF[] pts) : base(pts)
        {
            ;
        }

        public override void Draw(Renderer renderer)
        {
            renderer.DrawPolygon(Style, Points);
        }
    }
}
