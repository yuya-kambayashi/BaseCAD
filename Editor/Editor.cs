﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using System.Reflection;

namespace BaseCAD
{
    public class Editor
    {
        private static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public CADDocument Document { get; private set; }
        internal bool InputMode { get; set; } = false;
        internal SelectionSet CurrentSelection { get; private set; } = new SelectionSet();
        public SelectionSet PickedSelection { get; private set; } = new SelectionSet();
        public SnapPointType SnapMode { get => Document.Settings.SnapMode; }
        internal SnapPointCollection SnapPoints { get; set; } = new SnapPointCollection();

        static Editor()
        {
            // Search the assembly for commands
            Assembly assembly = Assembly.GetAssembly(typeof(CADDocument));
            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType == typeof(Command))
                {
                    Command com = (Command)Activator.CreateInstance(type);
                    if (com == null)
                    {
                        assembly = Assembly.GetExecutingAssembly();
                        com = (Command)Assembly.GetExecutingAssembly().CreateInstance(type.FullName);
                    }
                    if (com != null)
                    {
                        commands.Add(com.RegisteredName, com);
                    }
                }
            }
        }

        public Editor(CADDocument doc)
        {
            Document = doc;
        }

        public void DoPrompt(string message)
        {
            OnPrompt(new EditorPromptEventArgs(message));
        }

        public void RunCommand(string registeredName, params string[] args)
        {
            if (commands.ContainsKey(registeredName))
            {
                Command com = commands[registeredName];
                Command clearSelection = new Commands.SelectionClear();
                Task runTask = com.Apply(Document, args).ContinueWith(
                    (t) =>
                    {
                        if (t.IsFaulted)
                            OnError(new EditorErrorEventArgs(t.Exception));
                        else if (t.IsCompleted)
                            clearSelection.Apply(Document, args);
                    }
                );
            }
            else
            {
                OnError(new EditorErrorEventArgs(new InvalidOperationException("Unknown command name: " + registeredName)));
            }
        }

        #region Editor Getters
        public async Task<InputResult<string>> GetOpenFilename(string message)
        {
            return await GetOpenFilename(new FilenameOptions(message));
        }

        public async Task<InputResult<string>> GetOpenFilename(string message, string filename)
        {
            return await GetOpenFilename(new FilenameOptions(message, filename));
        }

        public async Task<InputResult<string>> GetOpenFilename(string message, string filename, string filter)
        {
            return await GetOpenFilename(new FilenameOptions(message, filename, filter));
        }

        public async Task<InputResult<string>> GetOpenFilename(string message, string filename, string filter, string ext)
        {
            return await GetOpenFilename(new FilenameOptions(message, filename, filter, ext));
        }

        public async Task<InputResult<string>> GetOpenFilename(FilenameOptions options)
        {
            return await OpenFilenameGetter.Run<OpenFilenameGetter>(this, options);
        }

        public async Task<InputResult<string>> GetSaveFilename(string message)
        {
            return await GetSaveFilename(new FilenameOptions(message));
        }

        public async Task<InputResult<string>> GetSaveFilename(string message, string filename)
        {
            return await GetSaveFilename(new FilenameOptions(message, filename));
        }

        public async Task<InputResult<string>> GetSaveFilename(string message, string filename, string filter)
        {
            return await GetSaveFilename(new FilenameOptions(message, filename, filter));
        }

        public async Task<InputResult<string>> GetSaveFilename(string message, string filename, string filter, string ext)
        {
            return await GetSaveFilename(new FilenameOptions(message, filename, filter, ext));
        }

        public async Task<InputResult<string>> GetSaveFilename(FilenameOptions options)
        {
            return await OpenFilenameGetter.Run<OpenFilenameGetter>(this, options);
        }

        public async Task<InputResult<SelectionSet>> GetSelection(string message)
        {
            return await GetSelection(new SelectionOptions(message));
        }

        public async Task<InputResult<SelectionSet>> GetSelection(SelectionOptions options)
        {
            CurrentSelection.Clear();

            while (true)
            {
                var result = await SelectionGetter.Run<SelectionGetter>(this, options);
                if (result.Result == ResultMode.Cancel && (result.CancelReason == CancelReason.Escape || result.CancelReason == CancelReason.Init))
                    return result;
                else if (result.Result == ResultMode.Cancel && (result.CancelReason == CancelReason.Enter || result.CancelReason == CancelReason.Space))
                    return InputResult<SelectionSet>.AcceptResult(CurrentSelection, AcceptReason.Coords);
                else if (result.Result == ResultMode.OK)
                    CurrentSelection.UnionWith(result.Value);

                if (result.Result == ResultMode.OK && result.AcceptReason == AcceptReason.Init)
                    return InputResult<SelectionSet>.AcceptResult(CurrentSelection, AcceptReason.Init);
            }
        }

        public async Task<InputResult<Point2D>> GetPoint(string message)
        {
            return await GetPoint(new PointOptions(message));
        }

        public async Task<InputResult<Point2D>> GetPoint(string message, Action<Point2D> jig)
        {
            return await GetPoint(new PointOptions(message, jig));
        }

        public async Task<InputResult<Point2D>> GetPoint(string message, Point2D basePoint)
        {
            return await GetPoint(new PointOptions(message, basePoint));
        }

        public async Task<InputResult<Point2D>> GetPoint(string message, Point2D basePoint, Action<Point2D> jig)
        {
            return await GetPoint(new PointOptions(message, basePoint, jig));
        }

        public async Task<InputResult<Point2D>> GetPoint(PointOptions options)
        {
            return await PointGetter.Run<PointGetter>(this, options);
        }

        public async Task<InputResult<Point2D>> GetCorner(string message, Point2D basePoint)
        {
            return await GetCorner(new CornerOptions(message, basePoint));
        }

        public async Task<InputResult<Point2D>> GetCorner(string message, Point2D basePoint, Action<Point2D> jig)
        {
            return await GetCorner(new CornerOptions(message, basePoint, jig));
        }

        public async Task<InputResult<Point2D>> GetCorner(CornerOptions options)
        {
            return await CornerGetter.Run<CornerGetter>(this, options);
        }

        public async Task<InputResult<float>> GetAngle(string message, Point2D basePoint, Action<float> jig)
        {
            return await GetAngle(new AngleOptions(message, basePoint, jig));
        }

        public async Task<InputResult<float>> GetAngle(string message, Point2D basePoint)
        {
            return await GetAngle(new AngleOptions(message, basePoint));
        }

        public async Task<InputResult<float>> GetAngle(AngleOptions options)
        {
            return await AngleGetter.Run<AngleGetter>(this, options);
        }

        public async Task<InputResult<float>> GetDistance(string message, Point2D basePoint, Action<float> jig)
        {
            return await GetDistance(new DistanceOptions(message, basePoint, jig));
        }

        public async Task<InputResult<float>> GetDistance(string message, Point2D basePoint)
        {
            return await GetDistance(new DistanceOptions(message, basePoint));
        }

        public async Task<InputResult<float>> GetDistance(DistanceOptions options)
        {
            return await DistanceGetter.Run<DistanceGetter>(this, options);
        }

        public async Task<InputResult<string>> GetText(string message, Action<string> jig)
        {
            return await GetText(new TextOptions(message, jig));
        }

        public async Task<InputResult<string>> GetText(string message)
        {
            return await GetText(new TextOptions(message));
        }

        public async Task<InputResult<string>> GetText(TextOptions options)
        {
            return await TextGetter.Run<TextGetter>(this, options);
        }

        public async Task<InputResult<int>> GetInt(string message, Action<int> jig)
        {
            return await GetInt(new IntOptions(message, jig));
        }

        public async Task<InputResult<int>> GetInt(IntOptions options)
        {
            return await IntGetter.Run<IntGetter>(this, options);
        }

        public async Task<InputResult<float>> GetFloat(string message, Action<float> jig)
        {
            return await GetFloat(new FloatOptions(message, jig));
        }

        public async Task<InputResult<float>> GetFloat(FloatOptions options)
        {
            return await FloatGetter.Run<FloatGetter>(this, options);
        }
        #endregion

        #region View Events
        internal event CursorEventHandler CursorMove;
        internal event CursorEventHandler CursorClick;
        internal event KeyEventHandler KeyDown;
        internal event KeyPressEventHandler KeyPress;
        internal void OnViewMouseMove(object sender, CursorEventArgs e)
        {
            CursorMove?.Invoke(sender, e);
        }

        internal void OnViewMouseClick(object sender, CursorEventArgs e)
        {
            CursorClick?.Invoke(sender, e);
        }

        internal void OnViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown?.Invoke(sender, e);
        }

        internal void OnViewKeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPress?.Invoke(sender, e);
        }
        #endregion

        #region Events
        public event EditorPromptEventHandler Prompt;
        public event EditorErrorEventHandler Error;

        protected void OnPrompt(EditorPromptEventArgs e)
        {
            Prompt?.Invoke(this, e);
        }
        protected void OnError(EditorErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        #endregion
    }
}
