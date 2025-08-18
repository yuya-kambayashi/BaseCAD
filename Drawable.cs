﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace BaseCAD
{
    public abstract class Drawable : INotifyPropertyChanged
    {
        public virtual Outline Outline { get; set; } = Outline.White;
        public virtual bool Visible { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Draw(DrawParams param);
        public abstract Extents2D GetExtents();
        public virtual bool Contains(Point2D pt, float pickBoxSize) { return GetExtents().Contains(pt); }
        public abstract void TransformBy(TransformationMatrix2D transformation);
        public virtual Drawable Clone() { return (Drawable)MemberwiseClone(); }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Drawable()
        {
            ;
        }
    }
}
