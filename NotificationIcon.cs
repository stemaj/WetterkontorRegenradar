using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Net;

namespace WetterkontorRegenradar
{
	public sealed class NotificationIcon
	{
		private readonly NotifyIcon _notifyIcon;
	    private Form _form;
		private PictureBox _pictureBox;
		private Button _button;
		DateTime _dateTimeAktuell;
		DateTime _dateTimeVorhersage;
		Image _imageAktuell;
		Image _imageVorhersage;

	    public NotificationIcon()
		{
	        _notifyIcon = new NotifyIcon();
			var notificationMenu = new ContextMenu(InitializeMenu());
			
			_notifyIcon.DoubleClick += IconDoubleClick;
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			_notifyIcon.Icon = (Icon)resources.GetObject("rain");
			_notifyIcon.ContextMenu = notificationMenu;
			_notifyIcon.Text = "Wetterkontor Regenradar";
		}
		
		private MenuItem[] InitializeMenu()
		{
			var menu = new MenuItem[] {
				new MenuItem("Aktuell", MenuRadarClick),
				new MenuItem("Vorhersage", MenuRadarClick),
				new MenuItem("Beenden", MenuExitClick)
			};
			return menu;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			bool isFirstInstance;
			using (var mtx = new Mutex(true, "WetterkontorRegenradar", out isFirstInstance))
			{
				if (isFirstInstance) 
				{
					var notificationIcon = new NotificationIcon();
					notificationIcon.DialogInitialisieren();
					notificationIcon._notifyIcon.Visible = true;
					Application.Run();
					notificationIcon._notifyIcon.Dispose();
				} 
				else 
				{
					MessageBox.Show("Wat für messags");
					// The application is already running
				}
			}
		}

	    private void MenuRadarClick(object sender, EventArgs e)
		{
	        var menuItem = sender as ButtonBase;
	        if (menuItem == null) return;
	        var menuText = menuItem.Text;

	        switch (menuText)
	        {
	            case "Aktuell":
	                DialogAnzeigen(0);
	                break;
	            case "Vorhersage":
	                DialogAnzeigen(1);
	                break;
	        }
		}

		void _form_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_imageAktuell != null)
				_imageAktuell.Dispose();
			_imageAktuell = null;
			if (_imageVorhersage != null)
				_imageVorhersage.Dispose();
			_imageVorhersage = null;
		}
		
		void DialogInitialisieren()
		{
			_form = new Form();
			_pictureBox = new PictureBox {Dock = DockStyle.Fill};
		    _form.Controls.Add(_pictureBox);
			_button = new Button {Dock = DockStyle.Bottom};
		    _button.Click += MenuRadarClick;
			_form.FormClosed += _form_FormClosed;
			_form.Controls.Add(_button);
		}
			
		void DialogAnzeigen(int tag)
		{
			if (_form.IsDisposed)
				DialogInitialisieren();
			
			if (tag == 0)
			{
				if (_imageAktuell == null || ((DateTime.Now - _dateTimeAktuell).TotalMinutes > 5))
				{
					const string url = "http://img.wetterkontor.de/radar/radar_aktuell.gif";
					_imageAktuell = GetImageFromUrl(url);
					_imageAktuell.Tag = 0;
					
					_dateTimeAktuell = DateTime.Now;
				}
				_pictureBox.Image = _imageAktuell;
				_form.Size = _imageAktuell.Size;
				_form.Text = "Radar";
				_button.Text = "Vorhersage";				
			}
			else
			{
				if (_imageVorhersage == null || ((DateTime.Now - _dateTimeVorhersage).TotalMinutes > 5))
				{
					const string url = "http://img.wetterkontor.de/radar/radar_vorhersage.gif";
					_imageVorhersage = GetImageFromUrl(url);
					_imageVorhersage.Tag = 1;
					
					_dateTimeVorhersage = DateTime.Now;
				}
				_pictureBox.Image = _imageVorhersage;
				_form.Size = _imageVorhersage.Size;
				_form.Text = "Vorhersage";
				_button.Text = "Aktuell";
			}

			_form.Width += 10;
			_form.Height += 60;
			_form.Show();
		}

		private static void MenuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e)
		{
			DialogAnzeigen(0);
		}

		private static Image GetImageFromUrl(string url)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
			var stream = httpWebReponse.GetResponseStream();
	        return stream != null ? Image.FromStream(stream) : null;
		}
	}
}
