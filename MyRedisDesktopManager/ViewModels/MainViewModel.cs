using MahApps.Metro.Controls.Dialogs;
using MyRedisDesktopManager.Models;
using MyRedisDesktopManager.Services;
using MyRedisDesktopManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyRedisDesktopManager.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private readonly IDialogCoordinator _dialogCoordinator;

		private readonly UIService _uIService = new UIService();
		private readonly RedisService _redisService = new RedisService();

		private object _selectedValuePath;

		public ICommand AddConnectionCommand => new Command(async (s) => await AddConnectionAsync());

		public ICommand AppSettingCommand => new Command(async (s) => await AppSettingAsync());

		public ICommand ConnectionClosedCommand => new Command(async (s) => await ConnectionClosed());

		public ICommand ConnectionEditCommand => new Command(async (s) => await ConnectionEditAsync(s));

		public ICommand ConnectionDeleteCommand => new Command(async (s) => await ConnectionDelete());


		public ICommand TreeviewSelectedItemChangedCommand => new Command(async (s) => await TreeviewSelectedItemChangedAsync(s));


		public object SelectedValuePath
		{
			get => _selectedValuePath; set
			{
				_selectedValuePath = value;
			}
		}


		public ObservableRangeCollection<RedisConnectModel> Connects { get; set; } = new ObservableRangeCollection<RedisConnectModel>();


		public MainViewModel()
		{
			_dialogCoordinator = DialogCoordinator.Instance;

			Init();
		}

		private void Init()
		{
			this.LoadRedisConnects();
		}

		private void LoadRedisConnects()
		{
			this.Connects.ReplaceRange(_uIService.GetRedisConnects());
		}

		private async Task AddConnectionAsync()
		{
			var dialog = new CustomDialog();

			dialog.Content = new EditConnectionView()
			{
				DataContext = new EditConnectionViewModel()
				{
					Close = async () => { await _dialogCoordinator.HideMetroDialogAsync(this, dialog); LoadRedisConnects(); }
				}
			};

			await _dialogCoordinator.ShowMetroDialogAsync(this, dialog);
		}

		private async Task AppSettingAsync()
		{
			//await _dialogCoordinator.ShowMetroDialogAsync(this, new CustomDialog() { Content = new AppSettingView() }); 
		}

		private async Task ConnectionEditAsync(object o)
		{
			if (o is RedisConnectModel model)
			{
				var dialog = new CustomDialog();

				dialog.Content = new EditConnectionView()
				{
					DataContext = new EditConnectionViewModel()
					{
						Guid = model.Guid,

						Host = model.ConnectionSetting.Host,
						Name = model.ConnectionSetting.Name,
						Password = model.ConnectionSetting.Password,
						Port = model.ConnectionSetting.Port,
						Timeout = model.ConnectionSetting.Timeout,

						Close = async () => { await _dialogCoordinator.HideMetroDialogAsync(this, dialog); LoadRedisConnects(); }
					}
				};

				await _dialogCoordinator.ShowMetroDialogAsync(this, dialog);
			}
			else
			{
				Debug.WriteLine("ConnectionEditAsync params is not 'RedisConnectModel' !");
			}


		}

		private Task ConnectionDelete()
		{
			throw new NotImplementedException();
		}

		private Task ConnectionClosed()
		{
			throw new NotImplementedException();
		}

		private async Task TreeviewSelectedItemChangedAsync(object o)
		{
			if (o is RedisConnectModel redisConnect)
			{
				if (!redisConnect.IsConnection)
				{
					redisConnect.IsLoading = true;

					try
					{
						var result = await _redisService.ConnectionAsync(redisConnect);

						if (result)
						{
							redisConnect.IsConnection = true;

							// 
							LoadDatabase(redisConnect);
						}

					}
					catch (Exception ex)
					{
						await _dialogCoordinator.ShowMessageAsync(this, "Connection", ex.Message);
					}

					redisConnect.IsLoading = false;
				}
				else { }
			}
			else if (o is RedisDbModel db)
			{
				if (db.HasLoadChidren)
					return;

				db.IsLoading = true;

				try
				{
					var keys = _redisService.GetKeys(db.RedisConnect.Guid, db.Index);

					db.KeyCount = keys.Item1;

					db.Keys.ReplaceRange(keys.Item2);

					db.HasLoadChidren = true;
				}
				catch (Exception ex)
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Load DB Keys", ex.Message);
				}

				db.IsLoading = false;
			}
		}

		private void LoadDatabase(RedisConnectModel connect)
		{
			var dbList = _redisService.GetRedisDbs(connect);

			connect.Databases.ReplaceRange(dbList);
		}

	}
}
