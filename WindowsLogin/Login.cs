using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.UserProfile;

namespace WindowsLogin
{
	public partial class Login : Form
	{

		public string GetRegValue(RegistryHive Hive, string SubKey, string Value)
		{
			var HiveKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
			var Key = HiveKey.OpenSubKey(SubKey);
			return (string)Key.GetValue(Value);
		}

		public string GetWallpaperOrder()
		{
			string SID = WindowsIdentity.GetCurrent().User.Value;
			return GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData\{SID}\AnyoneRead\LockScreen", "");
		}

		private string GetLockScreenPath()
		{
			string WallpaperOrder = GetWallpaperOrder();

			if(WallpaperOrder != null)
			{
				Debug.WriteLine(WallpaperOrder);
			}


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
