﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    public delegate void DocumentChangedEventHandler(object sender, EventArgs e);
    public delegate void TransientsChangedEventHandler(object sender, EventArgs e);
    public delegate void SelectionChangedEventHandler(object sender, EventArgs e);
}
