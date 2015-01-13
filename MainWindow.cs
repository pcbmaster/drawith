using System;
using System.Threading;
using Gtk;
using Meebey.SmartIrc4net;
using LewdChat;

public partial class MainWindow: Gtk.Window
{
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
		_IRCTHREAD.Start ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		irc.Disconnect ();
		Application.Quit ();
		a.RetVal = true;
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
}
