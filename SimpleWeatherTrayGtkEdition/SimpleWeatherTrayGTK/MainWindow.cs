using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Popup)
    {
        Build();
        this.GdkWindow.SkipTaskbarHint = true;
        this.GdkWindow.Hide();
        this.SkipTaskbarHint = true;
        this.Visible = false;
        this.Hide();
        this.HideAll();

        
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }
}
