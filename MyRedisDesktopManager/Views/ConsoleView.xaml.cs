using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyRedisDesktopManager.Views
{
	/// <summary>
	/// Interaction logic for ConsoleView.xaml
	/// </summary>
	public partial class ConsoleView : UserControl
	{
		ConsoleContent dc = new ConsoleContent();

		public ConsoleView()
		{
			InitializeComponent();

			this.DataContext = dc;

			InputBlock.KeyDown += InputBlock_KeyDown;
			InputBlock.Focus();

			Scroller.KeyDown += Scroller_KeyDown;

			dc.Enter = (input) => { };
		}

		private void Scroller_KeyDown(object sender, KeyEventArgs e)
		{
			InputBlock.Focus();
		}

		void InputBlock_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				dc.ConsoleInput = InputBlock.Text;
				dc.RunCommand();
				InputBlock.Focus();
				Scroller.ScrollToBottom();
			}
		}
	}

	public class ConsoleContent : INotifyPropertyChanged
	{
		private string consoleInput = string.Empty;

		private ObservableCollection<string> consoleOutputList = new ObservableCollection<string>();
		private string _consoleOutput;
		private string _current;

		public Action<string> Enter { get; set; }

		public string ConsoleInput
		{
			get
			{
				return consoleInput;
			}
			set
			{
				consoleInput = value;
				OnPropertyChanged("ConsoleInput");
			}
		}

		public ObservableCollection<string> ConsoleOutputList
		{
			get
			{
				return consoleOutputList;
			}
			set
			{
				consoleOutputList = value;
				OnPropertyChanged("ConsoleOutputList");
			}
		}


		public string Current { get => _current + ">"; set { _current = value; OnPropertyChanged("Current"); } }

		public string ConsoleOutput { get => _consoleOutput; set => _consoleOutput = value; }

		public ConsoleContent()
		{
			ConsoleOutputList.CollectionChanged += (sender, e) =>
			{
				_consoleOutput = string.Join("\n", consoleOutputList);
				OnPropertyChanged("ConsoleOutput");
			};
		}


		public void RunCommand()
		{
			ConsoleOutputList.Add(ConsoleInput);
			ConsoleInput = String.Empty;

			Enter?.Invoke(ConsoleInput);
		}


		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			if (null != PropertyChanged)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
