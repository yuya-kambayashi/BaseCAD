﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaseCAD
{
    public class CADDocument
    {
        [Browsable(false)]
        public Composite Model { get; private set; }
        [Browsable(false)]
        public Editor Editor { get; private set; }

        public event DocumentChangedEventHandler DocumentChanged;
        public event SelectionChangedEventHandler SelectionChanged;

        public CADDocument()
        {
            Editor = new Editor(this);
            Model = new Composite();
            Editor.Selection.CollectionChanged += Selection_CollectionChanged;
            Model.CollectionChanged += Model_CollectionChanged;
        }

        public void Open(string filename)
        {
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // old Code
                //IFormatter formatter = new BinaryFormatter();
                //Model.CollectionChanged -= Model_CollectionChanged;
                ////Model = (Composite)formatter.Deserialize(stream);
                //Model.CollectionChanged += Model_CollectionChanged;

                var json = File.ReadAllText(filename);
                Model.CollectionChanged -= Model_CollectionChanged;
                //Model = JsonSerializer.Deserialize<Composite>(json);
                Model.CollectionChanged += Model_CollectionChanged;
            }
        }
        public void Save(string filename)
        {
            using(Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // old Code
                //IFormatter formatter = new BinaryFormatter();
                ////formatter.Serialize(stream, Model);

                var json = JsonSerializer.Serialize(Model);
                //File.WriteAllText(filename, json);
            }
        }
        private void Model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnDocumentChanged(new EventArgs());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Editor.Selection.ExceptWith(e.OldItems.Cast<Drawable>());
                    OnDocumentChanged(new EventArgs());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Editor.Selection.Clear();
                    OnDocumentChanged(new EventArgs());
                    break;
            }
        }
        private void OnDocumentChanged(EventArgs e)
        {
            DocumentChanged?.Invoke(this, e);
        }
        private void Selection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDocumentChanged(new EventArgs());
        }
        private void OnSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

    }
}
