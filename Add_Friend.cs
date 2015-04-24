using System;

namespace LewdChat
{
	public partial class Add_Friend : Gtk.Dialog
	{
		public String Username, Greeting;

		public Add_Friend ()
		{
			this.Build ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnUsernameEntryChanged (object sender, EventArgs e)
		{
			Username = this.UsernameEntry.Text;
		}

		protected void OnGreetingEntryChanged (object sender, EventArgs e)
		{
			Greeting = this.GreetingEntry.Text;
		}
	}
}

