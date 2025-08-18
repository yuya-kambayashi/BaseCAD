﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    public partial class Command
    {
        public class TransformMove : Command
        {
            public override string RegisteredName => "Transform.Move";
            public override string Name => "Move";

            public override async Task Apply(CADDocument doc)
            {
                Editor ed = doc.Editor;

                Editor.SelectionResult s = await ed.GetSelection("Select objects: ");
                if (s.Result != Editor.ResultMode.OK || s.Value.Count == 0) return;
                Editor.PointResult p1 = await ed.GetPoint("Base point: ");
                if (p1.Result != Editor.ResultMode.OK) return;
                Composite consItems = new Composite();
                foreach (Drawable item in s.Value)
                {
                    consItems.Add(item.Clone());
                }
                consItems.Outline = doc.Editor.TransientStyle;
                consItems.CopyStyleToChildren();
                doc.Transients.Add(consItems);
                Point2D lastPt = p1.Value;
                Editor.PointResult p2 = await ed.GetPoint("Second point: ", p1.Value,
                    (p) => {
                        consItems.TransformBy(TransformationMatrix2D.Translation(p - lastPt));
                        lastPt = p;
                    });
                doc.Transients.Remove(consItems);
                if (p2.Result != Editor.ResultMode.OK) return;

                foreach (Drawable item in s.Value)
                {
                    item.TransformBy(TransformationMatrix2D.Translation(p2.Value - p1.Value));
                }

                ed.Selection.Clear();
            }
        }
        public class TransformRotate : Command
        {
            public override string RegisteredName => "Transform.Rotate";
            public override string Name => "Rotate";

            public override async Task Apply(CADDocument doc)
            {
                Editor ed = doc.Editor;

                Editor.SelectionResult s = await ed.GetSelection("Select objects: ");
                if (s.Result != Editor.ResultMode.OK || s.Value.Count == 0) return;
                Editor.PointResult p1 = await ed.GetPoint("Base point: ");
                if (p1.Result != Editor.ResultMode.OK) return;
                Composite consItems = new Composite();
                foreach (Drawable item in s.Value)
                {
                    consItems.Add(item.Clone());
                }
                consItems.Outline = doc.Editor.TransientStyle;
                consItems.CopyStyleToChildren();
                doc.Transients.Add(consItems);
                float lastAngle = 0;
                Editor.AngleResult p2 = await ed.GetAngle("Rotation angle: ", p1.Value,
                    (p) =>
                    {
                        consItems.TransformBy(TransformationMatrix2D.Rotation(p1.Value, p.Angle - lastAngle));
                        lastAngle = p.Angle;
                    });
                doc.Transients.Remove(consItems);
                if (p2.Result != Editor.ResultMode.OK) return;

                foreach (Drawable item in s.Value)
                {
                    item.TransformBy(TransformationMatrix2D.Rotation(p1.Value, p2.Value.Angle));
                }

                ed.Selection.Clear();
            }
        }

        public class TransformScale : Command
        {
            public override string RegisteredName => "Transform.Scale";
            public override string Name => "Scale";

            public override async Task Apply(CADDocument doc)
            {
                Editor ed = doc.Editor;

                Editor.SelectionResult s = await ed.GetSelection("Select objects: ");
                if (s.Result != Editor.ResultMode.OK || s.Value.Count == 0) return;
                Editor.PointResult p1 = await ed.GetPoint("Base point: ");
                if (p1.Result != Editor.ResultMode.OK) return;
                Composite consItems = new Composite();
                foreach (Drawable item in s.Value)
                {
                    consItems.Add(item.Clone());
                }
                consItems.Outline = doc.Editor.TransientStyle;
                consItems.CopyStyleToChildren();
                doc.Transients.Add(consItems);
                float lastScale = 1;
                Editor.DistanceResult p2 = await ed.GetDistance("Scale: ", p1.Value,
                    (p) =>
                    {
                        consItems.TransformBy(TransformationMatrix2D.Scale(p1.Value, p.Length / lastScale));
                        lastScale = p.Length;
                    });
                doc.Transients.Remove(consItems);
                if (p2.Result != Editor.ResultMode.OK) return;

                foreach (Drawable item in s.Value)
                {
                    item.TransformBy(TransformationMatrix2D.Scale(p1.Value, p2.Value));
                }

                ed.Selection.Clear();
            }
        }
    }
}
