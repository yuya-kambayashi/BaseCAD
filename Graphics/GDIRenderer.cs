﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Graphics
{
    public class GDIRenderer : Renderer
    {
        private System.Drawing.Graphics gdi;
        public override string Name => "GDI Renderer";

        public GDIRenderer(CADView view) : base(view)
        {
            ;
        }
        public override void Init(Control control)
        {
            try
            {
                // Enable double buffering
                Type type = control.GetType();
                MethodInfo method = type.GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(control, new object[] { ControlStyles.DoubleBuffer, true });
            }
            catch (System.Security.SecurityException)
            {
                ;
            }
        }

        public override void InitFrame(System.Drawing.Graphics graphics)
        {
            gdi = graphics;

            gdi.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gdi.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // Calculate model to view transformation
            gdi.ResetTransform();
            gdi.TranslateTransform(-View.Camera.Position.X, -View.Camera.Position.Y);
            gdi.ScaleTransform(1.0f / View.Camera.Zoom, -1.0f / View.Camera.Zoom, System.Drawing.Drawing2D.MatrixOrder.Append);
            gdi.TranslateTransform(View.Width / 2, View.Height / 2, System.Drawing.Drawing2D.MatrixOrder.Append);
        }

        public override void EndFrame()
        {
            ;
        }

        public override void Resize(int width, int height)
        {
            ;
        }

        protected override void Dispose(bool disposing)
        {
            ;
        }
        public override void Clear(Color color)
        {
            gdi.Clear(System.Drawing.Color.FromArgb((int)color.Argb));
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
            if (points.Count > 1)
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
            if (points.Count > 1)
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

        public override Vector2D MeasureString(string text, string fontFamily, FontStyle fontStyle, float textHeight)
        {
            // Revert transformation to identity while drawing text
            var oldMatrix = gdi.Transform;
            gdi.ResetTransform();

            // Calculate alignment in pixel coordinates
            float height = Math.Abs(View.WorldToScreen(new Vector2D(0, textHeight)).Y);
            Vector2D szWorld;
            using (var font = new System.Drawing.Font(fontFamily, height, System.Drawing.GraphicsUnit.Pixel))
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
                var pts = View.WorldToScreen(pt);

                // Revert transformation to identity while drawing text
                var oldTrans = gdi.Transform;
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
                gdi.TranslateTransform(pts.X, pts.Y, System.Drawing.Drawing2D.MatrixOrder.Append);

                gdi.DrawString(text, font, brush, 0, 0);

                // Restore old transformation
                gdi.Transform = oldTrans;
            }
        }

        public override void Draw(Drawable item)
        {
            item.Draw(this);
        }

        private System.Drawing.Pen CreatePen(Style style)
        {
            Style appliedStyle = (StyleOverride == null ? style : StyleOverride);

            var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb((int)(appliedStyle.Color.IsByLayer ? Color.White : appliedStyle.Color).Argb));
            pen.Width = GetScaledLineWeight(appliedStyle.LineWeight == Style.ByLayer ? 1 : appliedStyle.LineWeight);
            pen.DashStyle = appliedStyle.DashStyle == DashStyle.ByLayer ? System.Drawing.Drawing2D.DashStyle.Solid : (System.Drawing.Drawing2D.DashStyle)appliedStyle.DashStyle;
            return pen;
        }

        private System.Drawing.Brush CreateBrush(Style style)
        {
            Style appliedStyle = (StyleOverride == null ? style : StyleOverride);

            return new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)(appliedStyle.Color.IsByLayer ? Color.White : appliedStyle.Color).Argb));
        }
    }
}
