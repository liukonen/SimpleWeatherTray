using GLib;
using Gtk;
using Mono.Unix;
using Stetic;

public class MainWindow : Window
{
	public MainWindow()
		: this((WindowType)0)
	{
		Build();
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		((SignalArgs)a).set_RetVal((object)true);
	}

	protected virtual void Build()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		Gui.Initialize((Widget)(object)this);
		((Widget)this).set_Name("MainWindow");
		((Window)this).set_Title(Catalog.GetString("MainWindow"));
		((Window)this).set_WindowPosition((WindowPosition)4);
		if (((Bin)this).get_Child() != null)
		{
			((Bin)this).get_Child().ShowAll();
		}
		((Window)this).set_DefaultWidth(400);
		((Window)this).set_DefaultHeight(300);
		((Widget)this).Show();
		((Widget)this).add_DeleteEvent((DeleteEventHandler)(object)new DeleteEventHandler(OnDeleteEvent));
	}
}
