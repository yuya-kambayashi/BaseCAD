﻿using BaseCAD.Drawables;
using BaseCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    internal abstract class EditorGetter<TOptions, TValue> : IDisposable where TOptions : InputOptions<TValue>
    {
        private Drawable jigged = null;
        private string currentText = "";

        protected Editor Editor { get; private set; }
        protected TOptions Options { get; private set; }
        protected Drawable Jigged
        {
            get { return jigged; }
            set
            {
                if (jigged != null)
                    Editor.Document.Jigged.Remove(jigged);
                jigged = value;
                if (jigged != null)
                    Editor.Document.Jigged.Add(jigged);
            }
        }
        protected bool SpaceAccepts { get; set; } = true;
        protected TaskCompletionSource<InputResult<TValue>> Completion { get; private set; }

        public void Dispose()
        {
            Jigged = null;

            Editor.DoPrompt("");

            Editor.CursorMove -= Editor_CursorMove;
            Editor.CursorClick -= Editor_CursorClick;
            Editor.KeyDown -= Editor_KeyDown;
            Editor.KeyPress -= Editor_KeyPress;

            Editor.InputMode = false;
        }

        public static async Task<InputResult<TValue>> Run<T>(Editor editor, TOptions options) where T : EditorGetter<TOptions, TValue>
        {
            using (var getter = Activator.CreateInstance<T>())
            {
                getter.Editor = editor;

                getter.Completion = new TaskCompletionSource<InputResult<TValue>>();

                getter.Editor.InputMode = true;
                getter.Editor.DoPrompt("");

                getter.Options = options;

                var initArgs = new InitArgs<TValue>();
                getter.Init(initArgs);
                if (!initArgs.ContinueAsync)
                {
                    if (initArgs.InputValid)
                        getter.Completion.SetResult(InputResult<TValue>.AcceptResult(initArgs.Value));
                    else
                        getter.Completion.SetResult(InputResult<TValue>.CancelResult());
                }
                else
                {
                    await getter.InitAsync(initArgs);
                    if (!initArgs.ContinueAsync)
                    {
                        if (initArgs.InputValid)
                            getter.Completion.SetResult(InputResult<TValue>.AcceptResult(initArgs.Value));
                        else
                            getter.Completion.SetResult(InputResult<TValue>.CancelResult());
                    }
                }

                if (initArgs.ContinueAsync)
                {
                    getter.Editor.CursorMove += getter.Editor_CursorMove;
                    getter.Editor.CursorClick += getter.Editor_CursorClick;
                    getter.Editor.KeyDown += getter.Editor_KeyDown;
                    getter.Editor.KeyPress += getter.Editor_KeyPress;
                }

                return await getter.Completion.Task;
            }
        }

        private void Editor_CursorMove(object sender, CursorEventArgs e)
        {
            CoordsChanged(e.Location);
        }

        protected void SetCursorText(string text)
        {
            Editor.DoPrompt(Options.GetFullPrompt() + text);
        }

        private void Editor_CursorClick(object sender, CursorEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var args = new InputArgs<Point2D, TValue>(e.Location);
                AcceptCoordsInput(args);
                if (args.InputValid)
                {
                    Editor.DoPrompt("");
                    var result = InputResult<TValue>.AcceptResult(args.Value);
                    Completion.SetResult(result);
                }
                else
                {
                    currentText = "";
                    Editor.DoPrompt(Options.GetFullPrompt() + args.ErrorMessage);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right click equals return key
                Editor_KeyDown(sender, new KeyEventArgs(Keys.Return));
            }
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Editor.DoPrompt("");
                CancelInput();
                var result = InputResult<TValue>.CancelResult();
                Completion.SetResult(result);
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return || (SpaceAccepts && e.KeyCode == Keys.Space))
            {
                string keyword = Options.MatchKeyword(currentText);

                if (!string.IsNullOrEmpty(keyword))
                {
                    Editor.DoPrompt("");
                    var result = InputResult<TValue>.KeywordResult(keyword);
                    Completion.SetResult(result);
                }
                else
                {
                    var args = new InputArgs<string, TValue>(currentText);
                    AcceptTextInput(args);
                    if (args.InputValid)
                    {
                        Editor.DoPrompt("");
                        var result = InputResult<TValue>.AcceptResult(args.Value);
                        Completion.SetResult(result);
                    }
                    else
                    {
                        currentText = "";
                        Editor.DoPrompt(Options.GetFullPrompt() + args.ErrorMessage);
                    }
                }
            }
        }

        private void Editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool textChanged = false;

            if (e.KeyChar == '\b') // backspace
            {
                if (currentText.Length > 0)
                {
                    currentText = currentText.Remove(currentText.Length - 1);
                    textChanged = true;
                }
            }
            else if (e.KeyChar == ' ' && !SpaceAccepts)
            {
                currentText += e.KeyChar;
                textChanged = true;
            }
            else if (!char.IsControl(e.KeyChar))
            {
                currentText += e.KeyChar;
                textChanged = true;
            }

            if (textChanged)
            {
                Editor.DoPrompt(Options.GetFullPrompt() + currentText);
                TextChanged(currentText);
            }
        }

        protected virtual void Init(InitArgs<TValue> args) { }
        protected virtual Task InitAsync(InitArgs<TValue> args) { return Task.CompletedTask; }
        protected virtual void CoordsChanged(Point2D pt) { }
        protected virtual void TextChanged(string text) { }
        protected virtual void AcceptCoordsInput(InputArgs<Point2D, TValue> args) { }
        protected virtual void AcceptTextInput(InputArgs<string, TValue> args) { }
        protected virtual void CancelInput() { }
    }
}
