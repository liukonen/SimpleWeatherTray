using GLib;
using Gtk;
using Mono.Unix;
using Stetic;

public class MainWindow : Window
{
	public MainWindow(): base(WindowType.Popup)
	{
		Build();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		//((SignalArgs)a).set_RetVal((object)true);
	}

	protected virtual void Build()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		Gui.Initialize((Widget)(object)this);

		this.Name = "MainWindow";
		this.Title = "MainWindow";
		this.WindowPosition = WindowPosition.CenterOnParent;
		//this.ShowAll();

		this.DefaultWidth = 400;
		this.DefaultHeight = 300;

		this.Show();

		this.DeleteEvent += OnDeleteEvent;
	}
}
