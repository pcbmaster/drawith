using System;
using System.Threading;
using Gtk;
using Meebey.SmartIrc4net;
using LewdChat;
using Cairo;

public partial class MainWindow: Gtk.Window
{
	delegate void DrawShape(Cairo.Context ctx, PointD start, PointD end);

	ImageSurface surface;
	DrawingArea area;
	DrawShape Painter;
	PointD Start, End;
	Cairo.Color penColor;

	bool isDrawing;
	bool isDrawingPoint;

	public static IrcClient irc = new IrcClient();
	public string channel = "#drawith-test";
	public string nick = "Minty";
	public string realname = "MintyCat";

	public void Scroll2(object sender, Gtk.SizeAllocatedArgs e)
	{
		ChatWindow.ScrollToIter(ChatWindow.Buffer.EndIter, 0, false, 0, 0);
	}

	public void OnRawMessage(object sender, IrcEventArgs e)
	{
		//System.Console.WriteLine("Received: "+e.Data.RawMessage);
		Gtk.Application.Invoke (delegate {
			if(e.Data.Nick != null) {
				if(e.Data.Nick == nick) {
					ChatWindow.Buffer.InsertAtCursor ("Connected!\n");
				}
				else if (e.Data.Message.Contains("¤")) {
					System.Console.WriteLine("Image data!");
				}
				else {
					ChatWindow.Buffer.InsertAtCursor ("< " + e.Data.Nick + "> " + e.Data.Message + "\n");
				}
			}
		});
	}

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		Thread _IRCTHREAD = new Thread (new ThreadStart (IRCthread));
		ChatWindow.SizeAllocated += new SizeAllocatedHandler (Scroll2);
		ChatWindow.WrapMode = WrapMode.Word;
		surface = new ImageSurface (Format.Argb32, 300, 300);
		area = SendingCanvas;
		area.AddEvents (
			(int)Gdk.EventMask.PointerMotionMask
			| (int)Gdk.EventMask.ButtonPressMask
			| (int)Gdk.EventMask.ButtonReleaseMask);
		area.ExposeEvent += OnDrawingAreaExposed;
		area.ButtonPressEvent += OnMousePress;
		area.ButtonReleaseEvent += OnMouseRelease;
		area.MotionNotifyEvent += OnMouseMotion;
		DeleteEvent += delegate { Application.Quit(); };

		Painter = new DrawShape(DrawPoint);

		Start = new PointD(0.0, 0.0);
		End = new PointD(300.0, 300.0);
		isDrawing = false;
		isDrawingPoint = true;
		penColor = new Cairo.Color(0,0,0);

		_IRCTHREAD.Start ();
	}

	void OnDrawingAreaExposed(object source, ExposeEventArgs args)
	{
		Cairo.Context ctx;

		using (ctx = Gdk.CairoHelper.Create(area.GdkWindow))
		{
			ctx.Source = new SurfacePattern(surface);
			ctx.Paint();
		}

		if (isDrawing)
		{
			using (ctx = Gdk.CairoHelper.Create(area.GdkWindow))
			{
				Painter(ctx, Start, End);
			}
		}
	}


	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		irc.Disconnect ();
		Application.Quit ();
		a.RetVal = true;
	}

	void OnMousePress(object source, ButtonPressEventArgs args)
	{
		Start.X = args.Event.X;
		Start.Y = args.Event.Y;

		End.X = args.Event.X;
		End.Y = args.Event.Y;

		isDrawing = true;
		area.QueueDraw();
	}

	void OnMouseRelease(object source, ButtonReleaseEventArgs args)
	{
		End.X = args.Event.X;
		End.Y = args.Event.Y;

		isDrawing = false;

		using (Context ctx = new Context(surface))
		{
			Painter(ctx, Start, End);
		}

		area.QueueDraw();
	}

	void OnMouseMotion(object source, MotionNotifyEventArgs args)
	{
		if (isDrawing)
		{
			End.X = args.Event.X;
			End.Y = args.Event.Y;

			if(isDrawingPoint)
			{
				using (Context ctx = new Context(surface))
				{
					Painter(ctx, Start, End);
				}
			}
			area.QueueDraw();
		}
	}

	void PenClicked(object sender, EventArgs args)
	{
		isDrawingPoint = true;
		Painter = new DrawShape(DrawPoint);
	}

	void DrawLine(Cairo.Context ctx, PointD start, PointD end)
	{
		ctx.MoveTo(start);
		ctx.LineTo(end);
		ctx.Stroke();
	}

	void DrawPoint(Cairo.Context ctx, PointD start, PointD end)
	{
		ctx.SetSourceColor (penColor);
		ctx.Rectangle(end, 1, 1);
		ctx.Stroke();
	}

	protected void OnSendButtonClicked (object sender, EventArgs e)
	{
		irc.SendMessage (SendType.Message, channel, ChatEntry.Text);
		ChatWindow.Buffer.InsertAtCursor ("< " + nick + "> " + ChatEntry.Text + "\n");
		ChatEntry.Text = "";
	}

	void IRCthread(){
		string[] serverlist;
		irc.OnRawMessage += new IrcEventHandler (OnRawMessage);
		serverlist = new string[] { "irc.choopa.net" };
		int port = 6667;
		try {
			irc.Connect (serverlist, port);
		} catch (ConnectionException e) {
			Gtk.Application.Invoke (delegate {
				ChatWindow.Buffer.InsertAtCursor("Couldn't connect to server! Reason:" + e.Message);
			});
		}
		try {
			irc.Login (nick, realname);
			irc.RfcJoin (channel);
			irc.Listen ();
		} catch (ConnectionException) {
		}
	}

	protected void OnSendCanvasButtonClicked (object sender, EventArgs e)
	{

	}

	protected void OnColorbutton2ColorSet (object sender, EventArgs e)
	{
		penColor = ToCairoColor(colorbutton2.Color);
	}

	Cairo.Color ToCairoColor (Gdk.Color color) 
	{ 
		return new Cairo.Color ((double)color.Red / ushort.MaxValue, 
			(double)color.Green / ushort.MaxValue, (double)color.Blue / 
			ushort.MaxValue); 
	} 
}
