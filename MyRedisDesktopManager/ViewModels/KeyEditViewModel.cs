using MahApps.Metro.Controls.Dialogs;
using MyRedisDesktopManager.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MyRedisDesktopManager.ViewModels
{
	public class KeyEditViewModel : TabItemViewModel
	{
		private readonly IDialogCoordinator _dialogCoordinator;

		private RedisDbKeyValueModel _keyValue;
		private int _viewTypeSelect = 1;
		private string _resultViewText;


		public RedisDbKeyValueModel KeyValue
		{
			get { return _keyValue; }
			set { _keyValue = value; OnPropertyChanged(nameof(KeyValue)); RenderResultText(); }
		}

		public string ResultViewText
		{
			get { return _resultViewText; }
			set { _resultViewText = value; OnPropertyChanged(nameof(ResultViewText)); }
		}

		public int ViewTypeSelect
		{
			get { return _viewTypeSelect; }
			set { _viewTypeSelect = value; ViewTypeSelectChange(); }
		}

		public List<ViewType> ViewTypeList { get; set; } = new List<ViewType>() {
			new ViewType( 1, "PLAIN TEXT" ),
			new ViewType( 2,  "JSON" ),
			new ViewType( 3,  "HEX" ),
		};


		public ICommand ReloadCommand => new Command((s) => ReloadCommandAction?.Invoke());

		public Action ReloadCommandAction { get; set; }


		public ICommand RenameKeyCommand => new Command((s) => RenameKeyCommandAction?.Invoke());

		public Action RenameKeyCommandAction { get; set; }


		public ICommand DeleteKeyCommand => new Command((s) => DeleteKeyCommandAction?.Invoke());

		public Action DeleteKeyCommandAction { get; set; }


		public ICommand UpdateTTLCommand => new Command((s) => UpdateTTLCommandAction?.Invoke());

		public Action UpdateTTLCommandAction { get; set; }


		public ICommand UpdateValueCommand => new Command((s) =>
		{
			var text = this.ResultViewText;
			if (ViewTypeSelect == 2)
			{
				text = JsonFormatHelper.Minify(this.ResultViewText);
			}
			UpdateValueCommandAction?.Invoke(text);
		});

		public Action<string> UpdateValueCommandAction { get; set; }


		public KeyEditViewModel()
		{
			_dialogCoordinator = DialogCoordinator.Instance;
		}

		private void ViewTypeSelectChange()
		{
			RenderResultText();
		}

		private void RenderResultText()
		{
			if (this.KeyValue.RedisType == RedisType.String)
			{
				if (ViewTypeSelect == 1)
				{
					this.ResultViewText = KeyValue.Value;
				}
				else if (ViewTypeSelect == 2)
				{
					this.ResultViewText = JsonFormatHelper.Format(KeyValue.Value);
				}
				else
				{
					MessageBox.Show("Unsoppert show type ", "Title");
				}
			}
		}

		public override string ToString()
		{
			return Title;
		}


	}

	public class ViewType
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public ViewType()
		{

		}

		public ViewType(int id, string name)
		{
			Id = id;
			Name = name;
		}
	}

}
