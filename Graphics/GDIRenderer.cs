﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Graphics
{
    public class GDIRenderer : Renderer
    {
        private System.Drawing.Graphics gdi;

        public GDIRenderer(CADView view, System.Drawing.Graphics graphicsData) : base(view, graphicsData)
        {
            gdi = graphicsData;
        }

        public override void DrawLine(Style style, Point2D p1, Point2D p2)
        {
            using (var pen = CreatePen(style))
            {
                gdi.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
            }
        }

        public override void DrawRectangle(Style style, Point2D p1, Point2D p2)
        {
            if (style.Fill)
            {
                using (var brush = CreateBrush(style))
                {
                    gdi.FillRectangle(brush, Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                        Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
                }
            }
            else
            {
                using (var pen = CreatePen(style))
                {
                    gdi.DrawRectangle(pen, Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                        Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
                }
            }
        }

        public override void DrawCircle(Style style, Point2D center, float radius)
        {
            if (style.Fill)
            {
                using (var brush = CreateBrush(style))
                {
                    gdi.FillEllipse(brush, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                }
            }
            else
            {
                using (var pen = CreatePen(style))
                {
                    gdi.DrawEllipse(pen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);
                }
            }
        }

        public override void DrawArc(Style style, Point2D center, float radius, float startAngle, float endAngle)
        {
            using (var pen = CreatePen(style))
            {
                float sweepAngle = endAngle - startAngle;
                while (sweepAngle < 0) sweepAngle += 2 * MathF.PI;
                while (sweepAngle > 2 * MathF.PI) sweepAngle -= 2 * MathF.PI;
                gdi.DrawArc(pen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius, startAngle * 180 / MathF.PI, sweepAngle * 180 / MathF.PI);
            }
        }

        public override void DrawEllipse(Style style, Point2D center, float semiMajorAxis, float semiMinorAxis, float rotation)
        {
            using (var pen = CreatePen(style))
            {
                var matrix = gdi.Transform;
                gdi.TranslateTransform(center.X, center.Y);
                gdi.RotateTransform(rotation * 180 / MathF.PI);
                gdi.DrawEllipse(pen, -semiMajorAxis, -semiMinorAxis, 2 * semiMajorAxis, 2 * semiMinorAxis);
                gdi.Transform = matrix;
            }
        }

        public override void DrawEllipticArc(Style style, Point2D center, float semiMajorAxis, float semiMinorAxis, float startAngle, float endAngle, float rotation)
        {
            using (var pen = CreatePen(style))
            {
                var matrix = gdi.Transform;
                gdi.TranslateTransform(center.X, center.Y);
                gdi.RotateTransform(rotation * 180 / MathF.PI);
                float sweepAngle = endAngle - startAngle;
                while (sweepAngle < 0) sweepAngle += 2 * MathF.PI;
                while (sweepAngle > 2 * MathF.PI) sweepAngle -= 2 * MathF.PI;
                gdi.DrawArc(pen, -semiMajorAxis, -semiMinorAxis, 2 * semiMajorAxis, 2 * semiMinorAxis, startAngle * 180 / MathF.PI, sweepAngle * 180 / MathF.PI);
                gdi.Transform = matrix;
            }
        }

        public override void DrawPolyline(Style style, Point2DCollection points, bool closed)
        {
            if (points.Count > 0)
            {
                var pts = points.ToPointF();
                using (var pen = CreatePen(style))
                {
                    if (closed)
                        gdi.DrawPolygon(pen, pts);
                    else
                        gdi.DrawLines(pen, pts);
                }
            }
        }

        public override void DrawPolygon(Style style, Point2DCollection points)
        {
            if (points.Count > 0)
            {
                var pts = points.ToPointF();
                if (style.Fill)
                {
                    using (var brush = CreateBrush(style))
                    {
                        gdi.FillPolygon(brush, pts);
                    }
                }
                else
                {
                    using (var pen = CreatePen(style))
                    {
                        gdi.DrawPolygon(pen, pts);
                    }
                }
            }
        }

        public override Vector2D MeasureString(string text, string fontFamily, float textHeight)
        {
            // Revert transformation to identity while drawing text
            var oldMatrix = gdi.Transform;
            gdi.ResetTransform();

            // Calculate alignment in pixel coordinates
            float height = Math.Abs(View.WorldToScreen(new Vector2D(0, textHeight)).Y);
            Vector2D szWorld;
            using (var font = new Font(fontFamily, height, GraphicsUnit.Pixel))
            {
                var sz = gdi.MeasureString(text, font);
                szWorld = View.ScreenToWorld(new Vector2D(Math.Abs(sz.Width), Math.Abs(sz.Height)));
            }
            // Restore old transformation
            gdi.Transform = oldMatrix;

            return new Vector2D(Math.Abs(szWorld.X), Math.Abs(szWorld.Y));
        }

        public override void DrawString(Style style, Point2D pt, string text,
            string fontFamily, float textHeight, FontStyle fontStyle,
            float rotation, TextHorizontalAlignment hAlign, TextVerticalAlignment vAlign)
        {
            float height = Math.Abs(View.WorldToScreen(new Vector2D(0, textHeight)).Y);
            using (var font = new System.Drawing.Font(fontFamily, height, (System.Drawing.FontStyle)fontStyle, System.Drawing.GraphicsUnit.Pixel))
            using (var brush = CreateBrush(style))
            {
                // Convert the text alignment point (x, y) to pixel coordinates
                var pts = new System.Drawing.PointF[] { new System.Drawing.PointF(pt.X, pt.Y) };
                gdi.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.Device, System.Drawing.Drawing2D.CoordinateSpace.World, pts);
                float x = pts[0].X;
                float y = pts[0].Y;

                // Revert transformation to identity while drawing text
                var oldMatrix = gdi.Transform;
                gdi.ResetTransform();

                // Calculate alignment in pixel coordinates
                float dx = 0;
                float dy = 0;
                var sz = gdi.MeasureString(text, font);

                if (hAlign == TextHorizontalAlignment.Right)
                    dx = -sz.Width;
                else if (hAlign == TextHorizontalAlignment.Center)
                    dx = -sz.Width / 2;

                if (vAlign == TextVerticalAlignment.Bottom)
                    dy = -sz.Height;
                else if (vAlign == TextVerticalAlignment.Middle)
                    dy = -sz.Height / 2;

                gdi.TranslateTransform(dx, dy, System.Drawing.Drawing2D.MatrixOrder.Append);
                gdi.RotateTransform(-rotation * 180 / MathF.PI, System.Drawing.Drawing2D.MatrixOrder.Append);
                gdi.TranslateTransform(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);

                gdi.DrawString(text, font, brush, 0, 0);

                // Restore old transformation
                gdi.Transform = oldMatrix;
            }
        }

        public override void Draw(Drawable item)
        {
            var ex = item.GetExtents();
            DrawRectangle(new Style(Color.Red), ex.Ptmin, ex.Ptmax);
        }

        private Pen CreatePen(Style style)
        {
            if (StyleOverride != null)
            {
                var pen = new Pen(System.Drawing.Color.FromArgb(
                    StyleOverride.Color.A,
                    StyleOverride.Color.R,
                    StyleOverride.Color.G,
                    StyleOverride.Color.B
                ), GetScaledLineWeight(StyleOverride.LineWeight));

                pen.DashStyle = (System.Drawing.Drawing2D.DashStyle)style.DashStyle;
                return pen;
            }
            else
            {
                var pen = new Pen(System.Drawing.Color.FromArgb(
                    style.Color.A,
                    style.Color.R,
                    style.Color.G,
                    style.Color.B
                ), GetScaledLineWeight(style.LineWeight));

                pen.DashStyle = (System.Drawing.Drawing2D.DashStyle)style.DashStyle;
                return pen;
            }
        }

        private Brush CreateBrush(Style style)
        {
            if (StyleOverride != null)
            {
                return new SolidBrush(System.Drawing.Color.FromArgb(
                    StyleOverride.Color.A,
                    StyleOverride.Color.R,
                    StyleOverride.Color.G,
                    StyleOverride.Color.B
                ));
            }
            else
            {
                return new SolidBrush(System.Drawing.Color.FromArgb(
                    style.Color.A,
                    style.Color.R,
                    style.Color.G,
                    style.Color.B
                ));
            }
        }
    }
}
