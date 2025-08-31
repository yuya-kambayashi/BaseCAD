﻿using BaseCAD.Geometry;
using System.ComponentModel;

namespace BaseCAD.Drawables
{
    public class Polygon : Polyline
    {
        [Browsable(false)]
        public new bool Closed => true;

        public Polygon() : base() { }
        public Polygon(Point2DCollection pts) : base(pts) { }
        public Polygon(params Point2D[] pts) : base(pts) { }
        public Polygon(PointF[] pts) : base(pts) { }
    }
}
