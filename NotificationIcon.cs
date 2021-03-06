﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Helpers;
using System.Windows.Forms;
using System.Net;
using Microsoft.CSharp.RuntimeBinder;

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
	    private MenuItem[] _menu;
        System.Windows.Forms.Timer _timer;

	    public NotificationIcon()
		{
	        _notifyIcon = new NotifyIcon();
			var notificationMenu = new ContextMenu(InitializeMenu());
            _notifyIcon.MouseMove += NotifyIconOnMouseMove; 
			_notifyIcon.DoubleClick += IconDoubleClick;
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			_notifyIcon.Icon = (Icon)resources.GetObject("rain");
			_notifyIcon.ContextMenu = notificationMenu;
		}

	    private void NotifyIconOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
	    {
            if (string.IsNullOrEmpty(_notifyIcon.Text))
                _notifyIcon.Text = GetTemperature();

	        if (_timer == null)
	        {
	            _timer = new System.Windows.Forms.Timer();
                _timer.Tick += TimerOnTick;
	        }

	        if (_timer.Enabled)
                return;

	        _timer.Interval = 5000;
            _timer.Start();

            _notifyIcon.Text = GetTemperature();
	    }

	    private void TimerOnTick(object sender, EventArgs eventArgs)
	    {
	        _timer.Stop();
	    }

	    static object GetDynamicMember(object obj, string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }

		private MenuItem[] InitializeMenu()
		{
		    _menu = new MenuItem[] {
				new MenuItem("Aktuell", MenuRadarClick),
				new MenuItem("Vorhersage", MenuRadarClick),
				new MenuItem("Beenden", MenuExitClick)
			};
			return _menu;
		}

	    private string GetTemperature()
	    {
	        var ow = new OpenWeather();
	        var x = ow.GetWeather();
	        
            if (x == null)
	            return " - ";

	        var v = GetDynamicMember(x, "main");

	        if (v == null)
	            return Convert.ToInt32(GetDynamicMember(x, "cod")) == 404 ? "404 Error" : "unknown Error";

	        var vv = Convert.ToDouble(GetDynamicMember(v, "temp")) - 273.15;
	        var vvv = String.Format("{0:0.0}", vv);
            return vvv + " °C";
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
                    MessageBox.Show("The application is already running");
				}
			}
		}

	    private void MenuRadarClick(object sender, EventArgs e)
	    {
	        string menuText;
	        var menuItem = sender as MenuItem;
	        if (menuItem != null)
	        {
	            menuText = menuItem.Text;
	        }
	        else
	        {
	            var menuItem2 = sender as ButtonBase;
	            if (menuItem2 == null)
	                return;
	            menuText = menuItem2.Text;
	        }

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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
            _form.Icon = (Icon)resources.GetObject("rain");
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
				    if (_imageAktuell != null) _imageAktuell.Tag = 0;

				    _dateTimeAktuell = DateTime.Now;
				}
			    if (_imageAktuell != null)
			    {
			        _pictureBox.Image = _imageAktuell;
			        _form.Size = _imageAktuell.Size;
			    }
			    _form.Text = "Radar / " + "Aktuelle Temperatur: " + GetTemperature();
				_button.Text = "Vorhersage";				
			}
			else
			{
				if (_imageVorhersage == null || ((DateTime.Now - _dateTimeVorhersage).TotalMinutes > 5))
				{
					const string url = "http://img.wetterkontor.de/radar/radar_vorhersage.gif";
					_imageVorhersage = GetImageFromUrl(url);
				    if (_imageVorhersage != null) _imageVorhersage.Tag = 1;

				    _dateTimeVorhersage = DateTime.Now;
				}
			    if (_imageVorhersage != null)
			    {
			        _pictureBox.Image = _imageVorhersage;
			        _form.Size = _imageVorhersage.Size;
			    }
			    _form.Text = "Vorhersage / " + "Aktuelle Temperatur: " + GetTemperature();
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
		    try
		    {
		        var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
		        var httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse();
		        var stream = httpWebReponse.GetResponseStream();
		        return stream != null ? Image.FromStream(stream) : null;
		    }
		    catch (WebException)
		    {
		        return null;
		    }
		}
	}
}
