using System;
using AutoGestPro.src.UI.Windows;
using Gtk;

namespace AutoGestPro.src
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.AutoGestPro.AutoGestPro", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new LoginWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
