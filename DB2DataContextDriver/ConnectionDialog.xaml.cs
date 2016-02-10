using System.Windows;
using LINQPad.Extensibility.DataContext;

namespace DB2DataContextDriver
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
		#region Constructors

		public ConnectionDialog(IConnectionInfo cxInfo)
		{
			DataContext = new DB2Properties(cxInfo);
			Background = SystemColors.ControlBrush;
			InitializeComponent();
		}

		#endregion

		#region Event Handlers

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		#endregion
	}
}
