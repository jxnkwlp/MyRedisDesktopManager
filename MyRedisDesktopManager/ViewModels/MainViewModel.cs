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


		#region Connection Context Menu Command 

		public ICommand DatabaseReloadCommand => new Command<RedisConnectModel>(async (p) => await DatabaseReloadAsync(p));

		public ICommand ConnectionClosedCommand => new Command<RedisConnectModel>(async (p) => await ConnectionClosedAsync(p));

		public ICommand ConnectionEditCommand => new Command<RedisConnectModel>(async (p) => await ConnectionEditAsync(p));

		public ICommand ConnectionDeleteCommand => new Command<RedisConnectModel>(async (p) => await ConnectionDeleteAsync(p));

		#endregion

		#region Database Context Menu Command 

		public ICommand DBReloadCommand => new Command<RedisDbModel>(async (p) => await DBReloadCommandExecuteAsync(p));

		public ICommand DBKeyFilterCommand => new Command<RedisDbModel>(async (p) => await DBKeyFilterCommandExecute(p));

		public ICommand DBAddKeyCommand => new Command<RedisDbModel>(async (p) => await DBAddKeyCommandExecute(p));

		public ICommand DBFlushCommand => new Command<RedisDbModel>(async (p) => await DBFlushCommandExecute(p));

		#endregion

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
						AllowAdmin = model.ConnectionSetting.AllowAdmin,

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

		private async Task ConnectionDeleteAsync(RedisConnectModel model)
		{
			var dialog = await _dialogCoordinator.ShowMessageAsync(this, "Connection Delete", "Are you sure delete the connection?", style: MessageDialogStyle.AffirmativeAndNegative);

			if (dialog == MessageDialogResult.Affirmative)
			{
				if (model.IsConnection)
				{
					await _redisService.CloseConnectionAsync(model);
				}

				_redisService.RemoveConnection(model);

				Connects.Remove(model);
			}

		}

		private async Task ConnectionClosedAsync(RedisConnectModel model)
		{
			if (model.IsConnection)
			{
				await _redisService.CloseConnectionAsync(model);

				model.IsConnection = false;
				model.Databases.Clear();
			}

		}

		private async Task TreeviewSelectedItemChangedAsync(object o)
		{
			if (o is RedisConnectModel redisConnect)
			{
				await LoadDatabaseAsync(redisConnect);
			}
			else if (o is RedisDbModel db)
			{
				await LoadDBKeysAsync(db);
			}
		}

		private async Task LoadDatabaseAsync(RedisConnectModel connect, bool focus = false)
		{
			if (!connect.IsConnection || focus)
			{
				connect.IsLoading = true;

				try
				{
					var result = await _redisService.ConnectionAsync(connect);

					if (result)
					{
						connect.IsConnection = true;

						// get database 
						var dbList = _redisService.GetRedisDbs(connect);

						// replace list 
						connect.Databases.ReplaceRange(dbList);
					}

				}
				catch (Exception ex)
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Connection", ex.Message);
				}

				connect.IsLoading = false;
			}


		}

		private async Task LoadDBKeysAsync(RedisDbModel db, bool focus = false)
		{
			if (db.HasLoadChidren && !focus)
				return;

			db.IsLoading = true;

			try
			{
				if (db.Keys.Count > 0)
				{
					db.Keys.Clear();
				}

				var keys = await _redisService.GetKeysAsync(db.RedisConnect.Guid, db.Index);

				db.KeyCount = keys.Item1;
				db.HasLoadChidren = true;

				db.Keys.ReplaceRange(keys.Item2);

			}
			catch (Exception ex)
			{
				await _dialogCoordinator.ShowMessageAsync(this, "Load DB Keys", ex.Message);
			}

			db.IsLoading = false;
		}


		private async Task DatabaseReloadAsync(RedisConnectModel model)
		{
			await LoadDatabaseAsync(model, true);
		}


		private async Task DBReloadCommandExecuteAsync(RedisDbModel db)
		{
			await LoadDBKeysAsync(db, true);
		}

		private async Task DBKeyFilterCommandExecute(RedisDbModel db)
		{
			throw new NotImplementedException();
		}

		private async Task DBAddKeyCommandExecute(RedisDbModel db)
		{
			throw new NotImplementedException();
		}

		private async Task DBFlushCommandExecute(RedisDbModel db)
		{
			//if (!db.RedisConnect.ConnectionSetting.AllowAdmin)
			//{
			//	await _dialogCoordinator.ShowMessageAsync(this, "Action Result", "Only admin can do flush database.");

			//	return;
			//}

			var dialog = await _dialogCoordinator.ShowMessageAsync(this, "Confirm Action", "Are you sure flush all keys ?", style: MessageDialogStyle.AffirmativeAndNegative);

			if (dialog == MessageDialogResult.Affirmative)
			{
				db.IsLoading = true;

				if (db.HasLoadChidren)
				{
					db.Keys.Clear();
				}

				db.HasLoadChidren = false;

				try
				{
					await _redisService.FlushDBAsync(db);
				}
				catch (Exception ex)
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Action Result", ex.Message);
				}

				db.IsLoading = false;
			}
		}

	}
}
