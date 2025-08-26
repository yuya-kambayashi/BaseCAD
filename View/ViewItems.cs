﻿namespace BaseCAD.View
{
    internal class ViewItems
    {
        public Drawables.DrawableDictionary Foreground { get; } = new Drawables.DrawableDictionary();
        public Drawables.DrawableDictionary Background { get; } = new Drawables.DrawableDictionary();

        public Cursor Cursor { get => (Cursor)Foreground["Cursor"]; }
        public Grid Grid { get => (Grid)Background["Grid"]; }
        public Axes Axes { get => (Axes)Background["Axes"]; }

        public ViewItems()
        {
            Foreground.Add("Cursor", new Cursor());
            Background.Add("Grid", new Grid());
            Background.Add("Axes", new Axes());
        }
    }
}
