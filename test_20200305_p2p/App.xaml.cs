using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace test_20200305_p2p
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup( object sender, StartupEventArgs e )
		{
			MainWindow wnd = new MainWindow();

			MainModel.Instance.Parse( e.Args );

			wnd.Show();
		}
	}
}
