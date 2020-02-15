using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
		public Login()
		{
			InitializeComponent();
		}

		private void LogonImage_Click(object sender, EventArgs e)
		{
			LogonImage.Load(LockScreen.OriginalImageFile.LocalPath);
		}
	}
}
