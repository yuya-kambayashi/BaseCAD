﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using BaseCAD.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = BaseCAD.Graphics.Color;

namespace BaseCAD.View
{
    internal class Axes : Drawable
    {
        public override void Draw(Renderer renderer)
        {
            var view = renderer.View;
            var doc = view.Document;

            Extents2D bounds = view.GetViewPort();
            Color axisColor = doc.Settings.Get<Color>("AxisColor");

            renderer.DrawLine(new Style(axisColor), new Point2D(0, bounds.Ymin), new Point2D(0, bounds.Ymax));
            renderer.DrawLine(new Style(axisColor), new Point2D(bounds.Xmin, 0), new Point2D(bounds.Xmax, 0));
        }

        public override Extents2D GetExtents()
        {
            return Extents2D.Empty;
        }

        public override void TransformBy(Matrix2D transformation)
        {
            ;
        }
    }
}
