﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseCAD
{
    [Docking(DockingBehavior.Ask)]
    public partial class CADWindow : UserControl
    {
        private CADDocument doc;

        [Browsable(false)]
        public CADView View { get; private set; }
        [Browsable(false)]
        public CADDocument Document
        {
            get
            {
                return doc;
            }
            set
            {
                doc = value;
                if (View != null) View.Detach();
                View = new CADView(doc);
                View.Attach(this);
            }
        }

        public CADWindow()
        {
            InitializeComponent();

            DoubleBuffered = true;

            BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
