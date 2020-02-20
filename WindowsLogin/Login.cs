using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsLogin
{
	public partial class Login : Form
	{

		public object GetRegValue(RegistryHive Hive, string SubKey, string Value)
		{
			var HiveKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
			var Key = HiveKey.OpenSubKey(SubKey);
			if(Key == null)
			{
				return null;
			}
			return Key.GetValue(Value);
		}

		public string GetWallpaperOrder()
		{
			string SID = WindowsIdentity.GetCurrent().User.Value;
			return (string)GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData\{SID}\AnyoneRead\LockScreen", "");
		}

		public char GetWallpaperChar()
		{
			return GetWallpaperOrder().First();
		}

		public string SID()
		{
			return WindowsIdentity.GetCurrent().User.Value;
		}

		public string GetPictureWallpaperPath()
		{
			return $@"C:\ProgramData\Microsoft\Windows\SystemData\{SID()}\LockScreen_{GetWallpaperChar()}";
		}

		public string GetSlideshowPath()
		{
			string SlideShowLibrary = File.ReadAllText(@"C:\Users\02jli01.ADMFALUN\AppData\Local\Microsoft\Windows\LockScreenSlideshow\Slideshow.library-ms");
			var PathMatch = System.Text.RegularExpressions.Regex.Match(SlideShowLibrary, @"(?<=<url>)(.*)(?=<\/url>)");
			return PathMatch.Value;
		}

		public bool SlideShowEnabled()
		{
			return Convert.ToBoolean((int)GetRegValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Lock Screen", "SlideshowEnabled"));
		}

		public bool SpotlightWallpaper()
		{
			return Convert.ToBoolean((int)GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\Creative\{SID()}", "RotatingLockScreenEnabled"));
		}

		public bool DefaultWallpaper()
		{
			return GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData\{SID()}\AnyoneRead\LockScreen", "") == null;
		}


		private string GetLockScreenPath()
		{

			if (DefaultWallpaper())
			{
				Debug.WriteLine($"Default Wallpaper : {LockScreen.OriginalImageFile.LocalPath}");
			}else if (SlideShowEnabled())
			{
				Debug.WriteLine($"Slideshow of {GetSlideshowPath()} directory");
				foreach (var path in Directory.GetFiles(GetSlideshowPath()))
				{
					Debug.Write(path + " ");
				}
				Debug.Write("\n");
			}else if (SpotlightWallpaper())
			{
				Debug.WriteLine("Spotight Enabled, Choose random wallpaper");
			}

			//string WallpaperOrder = GetWallpaperOrder();

			//if(WallpaperOrder != null)
			//{
			//	Debug.WriteLine(WallpaperOrder);
			//}

			//Debug.WriteLine(SlideShowEnabled());
			//Debug.WriteLine(SpotlightWallpaper());
			//Debug.WriteLine(DefaultWallpaper());



			return "";
		}

		public Login()
		{
			InitializeComponent();
		}

		private void Login_Load(object sender, EventArgs e)
		{
			GetLockScreenPath();
			LogonImage.Load(LockScreen.OriginalImageFile.LocalPath);
		}
	}
}
