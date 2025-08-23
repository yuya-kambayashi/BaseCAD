﻿using BaseCAD;
using BaseCAD.Drawables;
using BaseCAD.Geometry;
using BaseCAD.Graphics;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Reflection;
using System.Windows.Forms;
using Ellipse = SharpDX.Direct2D1.Ellipse;

namespace BaseCAD.Graphics
{
    public class DirectXRenderer : Renderer
    {
        private Control control;

        public SharpDX.Direct2D1.Factory factory2D;
        public SharpDX.DirectWrite.Factory factoryDWrite;
        public WindowRenderTarget renderTarget;

        public override string Name => "DirectX Renderer";

        public DirectXRenderer(CADView view) : base(view)
        {
            ;
        }

        public override void Init(Control ctrl)
        {
            control = ctrl;

            try
            {
                // Disable double buffering
                Type type = control.GetType();
                MethodInfo method = type.GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(control, new object[] { ControlStyles.DoubleBuffer, false });
            }
            catch (System.Security.SecurityException)
            {
                ;
            }

            // Initialize Direct2D and DirectWrite
            factory2D = new SharpDX.Direct2D1.Factory();
            factoryDWrite = new SharpDX.DirectWrite.Factory();

            var properties = new HwndRenderTargetProperties();
            properties.Hwnd = control.Handle;
            properties.PixelSize = new SharpDX.Size2(control.ClientSize.Width, control.ClientSize.Height);
            properties.PresentOptions = PresentOptions.None;

            renderTarget = new WindowRenderTarget(factory2D, new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)), properties);
            renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
            renderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
        }

        public override void InitFrame(System.Drawing.Graphics graphics)
        {
            renderTarget.BeginDraw();

            // Set model-view transformation
            Matrix2D m1 = Matrix2D.Translation(-View.Camera.Position.X, -View.Camera.Position.Y);
            Matrix2D m2 = Matrix2D.Scale(1.0f / View.Camera.Zoom, -1.0f / View.Camera.Zoom);
            Matrix2D m3 = Matrix2D.Translation(View.Width / 2, View.Height / 2);
            renderTarget.Transform = M2dxM(m3 * m2 * m1);
        }

        public override void EndFrame()
        {
            renderTarget.EndDraw();
        }

        public override void Resize(int width, int height)
        {
            renderTarget.Resize(new SharpDX.Size2(control.ClientSize.Width, control.ClientSize.Height));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                    factory2D.Dispose();
                    factoryDWrite.Dispose();
                }
                renderTarget = null;
            }
        }

        public override void Clear(Color color)
        {
            renderTarget.Clear(C2dxC(color));
        }

        public override void DrawLine(Style style, Point2D p1, Point2D p2)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);
            renderTarget.DrawLine(P2dxV(p1), P2dxV(p2), brush, strokeWidth, strokeStyle);
        }

        public override void DrawRectangle(Style style, Point2D p1, Point2D p2)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

            if (style.Fill)
                renderTarget.FillRectangle(P2dxR(p1, p2), brush);
            else
                renderTarget.DrawRectangle(P2dxR(p1, p2), brush, strokeWidth, strokeStyle);
        }

        public override void DrawCircle(Style style, Point2D center, float radius)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

            if (style.Fill)
                renderTarget.FillEllipse(new Ellipse(P2dxV(center), radius, radius), brush);
            else
                renderTarget.DrawEllipse(new Ellipse(P2dxV(center), radius, radius), brush, strokeWidth, strokeStyle);
        }

        public override void DrawArc(Style style, Point2D center, float radius, float startAngle, float endAngle)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

            Point2D startPoint = center + radius * Vector2D.FromAngle(startAngle);

            var geometry = new PathGeometry(factory2D);
            var sink = geometry.Open();
            sink.BeginFigure(P2dxV(startPoint), new FigureBegin());
            float sweepAngle = endAngle - startAngle;
            while (sweepAngle < 0) sweepAngle += 2 * MathF.PI;
            while (sweepAngle > 2 * MathF.PI) sweepAngle -= 2 * MathF.PI;
            float p = MathF.PI * radius * sweepAngle;
            float curveLength = View.WorldToScreen(new Vector2D(p, 0)).X;
            int n = (int)Math.Max(4, curveLength / 4);
            float da = sweepAngle / n;
            float a = startAngle;
            for (int i = 1; i <= n; i++)
            {
                Vector2D dir = Vector2D.FromAngle(a);
                float x = center.X + radius * dir.X;
                float y = center.Y + radius * dir.Y;
                sink.AddLine(new SharpDX.Mathematics.Interop.RawVector2(x, y));
                a += da;
            }
            sink.EndFigure(new FigureEnd());
            sink.Close();

            renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
        }

        public override void DrawEllipse(Style style, Point2D center, float semiMajorAxis, float semiMinorAxis, float rotation)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

            var trans = renderTarget.Transform;
            var transM = new Matrix2D(trans.M11, trans.M12, trans.M21, trans.M22, trans.M31, trans.M32);
            var m1 = Matrix2D.Translation(center.X, center.Y);
            var m2 = Matrix2D.Rotation(rotation);

            renderTarget.Transform = M2dxM(transM * m1 * m2);
            if (style.Fill)
                renderTarget.FillEllipse(new Ellipse(new SharpDX.Mathematics.Interop.RawVector2(0, 0), semiMajorAxis, semiMinorAxis), brush);
            else
                renderTarget.DrawEllipse(new Ellipse(new SharpDX.Mathematics.Interop.RawVector2(0, 0), semiMajorAxis, semiMinorAxis), brush, strokeWidth, strokeStyle);
            renderTarget.Transform = trans;
        }

        public override void DrawEllipticArc(Style style, Point2D center, float semiMajorAxis, float semiMinorAxis, float startAngle, float endAngle, float rotation)
        {
            var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

            float dxStart = MathF.Cos(startAngle) * semiMinorAxis;
            float dyStart = MathF.Sin(startAngle) * semiMajorAxis;
            float tStart = MathF.Atan2(dyStart, dxStart);

            float xStart = semiMajorAxis * MathF.Cos(tStart);
            float yStart = semiMinorAxis * MathF.Sin(tStart);
            Point2D startPoint = new Point2D(xStart, yStart);

            var geometry = new PathGeometry(factory2D);
            var sink = geometry.Open();
            sink.BeginFigure(P2dxV(startPoint), new FigureBegin());
            float sweepAngle = endAngle - startAngle;
            while (sweepAngle < 0) sweepAngle += 2 * MathF.PI;
            while (sweepAngle > 2 * MathF.PI) sweepAngle -= 2 * MathF.PI;
            float p = 2 * MathF.PI * (3 * (semiMajorAxis + semiMinorAxis) - MathF.Sqrt((3 * semiMajorAxis + semiMinorAxis) * (semiMajorAxis + 3 * semiMinorAxis)));
            float curveLength = View.WorldToScreen(new Vector2D(p, 0)).X;
            int n = (int)Math.Max(4, curveLength / 4);
            float da = sweepAngle / n;
            float a = startAngle;
            for (int i = 1; i <= n; i++)
            {
                float dx = MathF.Cos(a) * semiMinorAxis;
                float dy = MathF.Sin(a) * semiMajorAxis;
                float t = MathF.Atan2(dy, dx);

                float x = semiMajorAxis * MathF.Cos(t);
                float y = semiMinorAxis * MathF.Sin(t);

                sink.AddLine(new SharpDX.Mathematics.Interop.RawVector2(x, y));
                a += da;
            }
            sink.EndFigure(new FigureEnd());
            sink.Close();

            var trans = renderTarget.Transform;
            var transM = new Matrix2D(trans.M11, trans.M12, trans.M21, trans.M22, trans.M31, trans.M32);
            var m1 = Matrix2D.Translation(center.X, center.Y);
            var m2 = Matrix2D.Rotation(rotation);

            renderTarget.Transform = M2dxM(transM * m1 * m2);
            renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
            renderTarget.Transform = trans;
        }

        public override void DrawPolyline(Style style, Point2DCollection points, bool closed)
        {
            if (points.Count > 1)
            {
                var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

                var geometry = new PathGeometry(factory2D);
                var sink = geometry.Open();
                sink.BeginFigure(P2dxV(points[0]), new FigureBegin());

                for (int i = 1; i < points.Count; i++)
                {
                    sink.AddLine(P2dxV(points[i]));
                }
                if (closed)
                    sink.AddLine(P2dxV(points[0]));

                sink.EndFigure(new FigureEnd());
                sink.Close();

                renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
            }
        }

        public override void DrawPolygon(Style style, Point2DCollection points)
        {
            if (points.Count > 1)
            {
                var brush = CreateBrush(style, out var strokeWidth, out var strokeStyle);

                var geometry = new PathGeometry(factory2D);
                var sink = geometry.Open();
                sink.BeginFigure(P2dxV(points[0]), new FigureBegin());

                for (int i = 1; i < points.Count; i++)
                {
                    sink.AddLine(P2dxV(points[i]));
                }
                sink.AddLine(P2dxV(points[0]));

                sink.EndFigure(new FigureEnd());
                sink.Close();

                if (style.Fill)
                {
                    renderTarget.FillGeometry(geometry, brush);
                }
                else
                {
                    renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
                }
            }
        }

        public override Vector2D MeasureString(string text, string fontFamily, FontStyle fontStyle, float textHeight)
        {
            float height = Math.Abs(View.WorldToScreen(new Vector2D(0, textHeight)).Y);
            var format = CreateTextFormat(fontFamily, height, fontStyle);
            return MeasureString(text, format);
        }

        private Vector2D MeasureString(string text, TextFormat format)
        {
            var layout = new TextLayout(factoryDWrite, text, format, 1e10f, format.FontSize);
            Vector2D worldSize = View.ScreenToWorld(new Vector2D(Math.Abs(layout.Metrics.Width), Math.Abs(layout.Metrics.Height)));
            return new Vector2D(Math.Abs(worldSize.X), Math.Abs(worldSize.Y));
        }

        public override void DrawString(Style style, Point2D pt, string text, string fontFamily, float textHeight, FontStyle fontStyle, float rotation, TextHorizontalAlignment hAlign, TextVerticalAlignment vAlign)
        {
            float height = Math.Abs(View.WorldToScreen(new Vector2D(0, textHeight)).Y);
            var brush = CreateBrush(style, out _, out _);
            // Convert the text alignment point (x, y) to pixel coordinates
            var pts = View.WorldToScreen(pt);

            // Revert transformation to identity while drawing text
            var oldTrans = renderTarget.Transform;
            renderTarget.Transform = M2dxM(Matrix2D.Identity);
            var format = CreateTextFormat(fontFamily, height, fontStyle);
            var layout = new TextLayout(factoryDWrite, text, format, 1e10f, 1e10f);
            var sz = new Vector2D(Math.Abs(layout.Metrics.Width), Math.Abs(layout.Metrics.Height));
            var layoutRect = P2dxR(Point2D.Zero, sz.AsPoint2D());

            // Calculate alignment offset
            float dx = 0;
            float dy = 0;

            if (hAlign == TextHorizontalAlignment.Right)
                dx = -sz.X;
            else if (hAlign == TextHorizontalAlignment.Center)
                dx = -sz.X / 2;

            if (vAlign == TextVerticalAlignment.Middle)
                dy = -sz.Y / 2;
            else if (vAlign == TextVerticalAlignment.Bottom)
                dy = -sz.Y;

            var mat1 = SharpDX.Matrix3x2.Translation(dx, dy);
            var mat2 = SharpDX.Matrix3x2.Rotation(-rotation);
            var mat3 = SharpDX.Matrix3x2.Translation(pts.X, pts.Y);
            renderTarget.Transform = mat1 * mat2 * mat3;

            renderTarget.DrawText(text, format, layoutRect, brush);

            // Restore old transformation
            renderTarget.Transform = oldTrans;
        }

        public override void Draw(Drawable item)
        {
            item.Draw(this);
        }

        private SolidColorBrush CreateBrush(Style style, out float strokeWidth, out StrokeStyle strokeStyle)
        {
            Style appliedStyle = StyleOverride ?? style;

            strokeWidth = Math.Max(1, GetScaledLineWeight(appliedStyle.LineWeight));

            StrokeStyleProperties props = new StrokeStyleProperties();
            props.DashStyle = (SharpDX.Direct2D1.DashStyle)appliedStyle.DashStyle;
            strokeStyle = new StrokeStyle(factory2D, props);

            return new SolidColorBrush(renderTarget, C2dxC(appliedStyle.Color));
        }

        private TextFormat CreateTextFormat(string fontFamily, float textHeight, FontStyle fontStyle)
        {
            var format = new TextFormat(factoryDWrite, fontFamily, textHeight);

            return format;
        }

        private SharpDX.Mathematics.Interop.RawVector2 P2dxV(Point2D v)
        {
            return new SharpDX.Mathematics.Interop.RawVector2(v.X, v.Y);
        }

        private SharpDX.Mathematics.Interop.RawRectangleF P2dxR(Point2D v1, Point2D v2)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF(v1.X, v1.Y, v2.X, v2.Y);
        }

        private SharpDX.Mathematics.Interop.RawColor4 C2dxC(Color v)
        {
            return new SharpDX.Mathematics.Interop.RawColor4(v.R / 255f, v.G / 255f, v.B / 255f, v.A / 255f);
        }

        private SharpDX.Mathematics.Interop.RawMatrix3x2 M2dxM(Matrix2D v)
        {
            return new SharpDX.Mathematics.Interop.RawMatrix3x2(v.M11, v.M12, v.M21, v.M22, v.DX, v.DY);
        }
    }
}