﻿using System;
using System.Windows.Controls;

namespace Mystifier.JSVM
{
    internal class JSConsole
    {
        private readonly TextBox _outputBox;

        public JSConsole(TextBox outputBox)
        {
            _outputBox = outputBox;
        }

        public async void WriteLine(string format, params object[] args)
        {
            await
                _outputBox.Dispatcher.BeginInvoke(
                    new Action(() => { _outputBox.AppendText(string.Format(format, args) + "\n"); }));
        }

        public async void WriteLine(string str)
        {
            await _outputBox.Dispatcher.BeginInvoke(new Action(() => { _outputBox.AppendText(str + "\n"); }));
        }

        // ReSharper disable once InconsistentNaming
        public void log(object obj)
        {
            WriteLine(obj.ToString());
        }

        // ReSharper disable once InconsistentNaming
        public async void clear()
        {
            await _outputBox.Dispatcher.BeginInvoke(new Action(() => { _outputBox.Text = ""; }));
        }
    }
}