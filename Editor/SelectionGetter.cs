﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using BaseCAD.Graphics;
using Color = BaseCAD.Graphics.Color;

namespace BaseCAD
{
    internal class SelectionGetter : EditorGetter<SelectionOptions, SelectionSet>
    {
        Point2D firstPoint;
        bool getFirstPoint;
        Hatch consHatch;
        Polygon consLine;

        protected override void Init(InitArgs<SelectionSet> args)
        {
            // Immediately return existing picked-selection if any
            if (Editor.PickedSelection.Count != 0)
            {
                Editor.CurrentSelection.Clear();
                foreach (Drawable item in Editor.PickedSelection)
                {
                    Editor.CurrentSelection.Add(item);
                }
                Editor.PickedSelection.Clear();
                args.Value = Editor.CurrentSelection;
                args.ContinueAsync = false;
            }
            else
            {
                getFirstPoint = false;
            }
        }

        protected override void CoordsChanged(Point2D pt)
        {
            SetCursorText(pt.ToString(Editor.Document.Settings.NumberFormat));

            if (getFirstPoint)
            {
                // Update the selection window
                Point2D p1 = consLine.Points[0];
                Point2D p2 = new Point2D(pt.X, p1.Y);
                Point2D p3 = pt;
                Point2D p4 = new Point2D(p1.X, pt.Y);
                consLine.Points[0] = p1;
                consLine.Points[1] = p2;
                consLine.Points[2] = p3;
                consLine.Points[3] = p4;
                consHatch.Points[0] = p1;
                consHatch.Points[1] = p2;
                consHatch.Points[2] = p3;
                consHatch.Points[3] = p4;
                if (pt.X > p1.X)
                {
                    consHatch.Style = new Style(Editor.Document.Settings.Get<Color>("SelectionWindowColor"));
                    consLine.Style = new Style(Editor.Document.Settings.Get<Color>("SelectionWindowBorderColor"));
                }
                else
                {
                    consHatch.Style = new Style(Editor.Document.Settings.Get<Color>("ReverseSelectionWindowColor"));
                    consLine.Style = new Style(Editor.Document.Settings.Get<Color>("SelectionWindowBorderColor"), 0, DashStyle.Dash);
                }
            }
        }

        protected override void AcceptCoordsInput(InputArgs<Point2D, SelectionSet> args)
        {
            if (!getFirstPoint)
            {
                firstPoint = args.Input;
                getFirstPoint = true;
                args.InputCompleted = false;

                consHatch = new Hatch(firstPoint, firstPoint, firstPoint, firstPoint);
                Editor.Document.Transients.Add(consHatch);
                consLine = new Polygon(firstPoint, firstPoint, firstPoint, firstPoint);
                Editor.Document.Transients.Add(consLine);
            }
            else
            {
                args.Value = GetSelectionFromWindow();
                args.InputCompleted = true;

                Editor.Document.Transients.Remove(consHatch);
                Editor.Document.Transients.Remove(consLine);
            }
        }

        protected override void AcceptTextInput(InputArgs<string, SelectionSet> args)
        {
            args.InputValid = Point2D.TryParse(args.Input, out Point2D pt);
            if (args.InputValid)
            {
                if (!getFirstPoint)
                {
                    firstPoint = pt;
                    getFirstPoint = true;
                    args.InputCompleted = false;

                    consHatch = new Hatch(firstPoint, firstPoint, firstPoint, firstPoint);
                    Editor.Document.Transients.Add(consHatch);
                    consLine = new Polygon(firstPoint, firstPoint, firstPoint, firstPoint);
                    Editor.Document.Transients.Add(consLine);
                }
                else
                {
                    args.Value = GetSelectionFromWindow();
                    args.InputCompleted = true;

                    Editor.Document.Transients.Remove(consHatch);
                    Editor.Document.Transients.Remove(consLine);
                }
            }
        }

        protected override void CancelInput()
        {
            Editor.Document.Transients.Remove(consHatch);
            Editor.Document.Transients.Remove(consLine);
        }

        private SelectionSet GetSelectionFromWindow()
        {
            Editor.CurrentSelection.Clear();
            Extents2D ex = consHatch.GetExtents();
            bool windowSelection = (consHatch.Points[2].X > consHatch.Points[0].X);
            foreach (Drawable item in Editor.Document.Model)
            {
                Extents2D exItem = item.GetExtents();
                if (windowSelection && ex.Contains(exItem) || !windowSelection && ex.IntersectsWith(exItem))
                    Editor.CurrentSelection.Add(item);
            }
            return Editor.CurrentSelection;
        }
    }
}
