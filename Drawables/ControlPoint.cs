﻿using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD.Drawables
{
    public class ControlPoint
    {
        public enum ControlPointType
        {
            Point,
            Angle,
            Distance
        }
        public string Name { get; private set; }
        public ControlPointType Type { get; private set; }
        public Point2D BasePoint { get; private set; }
        public Point2D Location { get; private set; }
        internal int Index;
        public ControlPoint(string name, Point2D basePoint)
                : this(name, ControlPointType.Point, basePoint, basePoint)
        {
            ;
        }
        public ControlPoint(string name, ControlPointType type, Point2D basePoint, Point2D location)
        {
            Name = name;
            Type = type;
            BasePoint = basePoint;
            Location = location;
        }
    }
}
