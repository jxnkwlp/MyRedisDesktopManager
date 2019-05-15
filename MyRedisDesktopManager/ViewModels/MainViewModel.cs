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
using System.Windows;
using System.Windows.Input;

namespace MyRedisDesktopManager.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private readonly IDialogCoordinator _dialogCoordinator;

		private readonly UIService _uIService = new UIService();
		private readonly RedisService _redisService = new RedisService();

		private object _selectedValuePath;
		private KeyEditViewModel _tabSelect;

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

		#region Key COntext Menu Command

		public ICommand KeyCopyCommand => new Command<RedisDbKeyModel>(async (p) => await KeyCopyCommandExecuteAsync(p));

		public ICommand KeyDeleteCommand => new Command<RedisDbKeyModel>(async (p) => await KeyDeleteCommandExecuteAsync(p));

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

		public ObservableCollection<KeyEditViewModel> Tabs { get; set; } = new ObservableCollection<KeyEditViewModel>();

		public KeyEditViewModel TabSelect
		{
			get { return _tabSelect; }
			set { _tabSelect = value; OnPropertyChanged(nameof(TabSelect)); }
		}


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
				RemoveTabItems(model.Guid);

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

				RemoveTabItems(model.Guid);
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

			RemoveTabItems(model.Guid);
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
			else if (o is RedisDbKeyModel key)
			{
				if (!key.HasChildren)
					await LoadDBKeysValueAsync(key);
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

				var keys = await _redisService.GetKeysAsync(db.RedisConnect.Guid, db);

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
			RemoveTabItems(model.Guid);
			await LoadDatabaseAsync(model, true);
		}


		private async Task DBReloadCommandExecuteAsync(RedisDbModel db)
		{
			RemoveTabItems(db.RedisConnect.Guid);
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

				RemoveTabItems(db.RedisConnect.Guid);
			}
		}

		private async Task LoadDBKeysValueAsync(RedisDbKeyModel key)
		{
			if (key.Deleted) return;

			var id = key.RedisDb.RedisConnect.Guid + "_" + key.RedisDb.Index + "_" + key.FullKey;

			var tabItem = this.Tabs.FirstOrDefault(t => t.Id.StartsWith(key.RedisDb.RedisConnect.Guid.ToString()));

			var value = await LoadKeyValueAsync(key);

			if (value.RedisType == RedisType.None)
			{
				await _dialogCoordinator.ShowMessageAsync(this, $"The Key '{key.FullKey}' not found. ", "");
				return;
			}
			else if (value.RedisType == RedisType.Unknown)
			{
				await _dialogCoordinator.ShowMessageAsync(this, $"The Key '{key.FullKey}' type unsupport. ", "");
				return;
			}


			if (tabItem == null)
			{
				tabItem = new KeyEditViewModel()
				{
					Id = id,
					Title = key.RedisDb.RedisConnect.ConnectionSetting.Name + ":DB" + key.RedisDb.Index,

					KeyValue = value,
				};

				tabItem.ReloadCommandAction = async () => await KeyValueReloadCommandAsync(key);
				tabItem.RenameKeyCommandAction = async () => await KeyValueRenameKeyCommandActionAsync(key);
				tabItem.DeleteKeyCommandAction = async () => await KeyValueDeleteKeyCommandActionAsync(key);
				tabItem.UpdateTTLCommandAction = async () => await KeyValueUpdateTTLCommandActionAsync(key);
				tabItem.UpdateValueCommandAction = async (newText) => await KeyValueUpdateValueCommandActionAsync(key, newText);


				this.Tabs.Add(tabItem);
				this.TabSelect = tabItem;
			}
			else
			{
				tabItem.Id = id;
				tabItem.Title = key.RedisDb.RedisConnect.ConnectionSetting.Name + ":DB" + key.RedisDb.Index;
				tabItem.KeyValue = value;

				this.TabSelect = tabItem;
			}

		}


		private async Task<RedisDbKeyValueModel> LoadKeyValueAsync(RedisDbKeyModel key)
		{
			var value = await _redisService.GetKeyValueAsync(key);
			return value;
		}

		private async Task KeyValueReloadCommandAsync(RedisDbKeyModel key)
		{
			if (key.Deleted)
				return;

			var progress = await _dialogCoordinator.ShowProgressAsync(this, "Title", "Loading ...", true);

			var id = key.KeyId;
			var tabItem = this.Tabs.FirstOrDefault(t => t.Id.StartsWith(key.RedisDb.RedisConnect.Guid.ToString()));

			if (tabItem == null)
			{
				await progress.CloseAsync();
				return;
			}

			try
			{
				var value = await LoadKeyValueAsync(key);

				tabItem.KeyValue = value;

				await progress.CloseAsync();
			}
			catch (Exception ex)
			{
				await progress.CloseAsync().ContinueWith(async (_) =>
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Title", ex.Message);
				});

			}

		}

		private async Task KeyValueRenameKeyCommandActionAsync(RedisDbKeyModel key)
		{
			var id = key.KeyId;
			var tabItem = this.Tabs.FirstOrDefault(t => t.Id.StartsWith(key.RedisDb.RedisConnect.Guid.ToString()));

			var input = await _dialogCoordinator.ShowInputAsync(this, "Please input new key", "", new MetroDialogSettings() { DefaultText = key.FullKey, });

			if (string.IsNullOrWhiteSpace(input))
			{
				return;
			}

			var result = await _redisService.KeyRenameAsync(key, input);

			if (result)
			{
				if (tabItem != null)
				{
					this.Tabs.Remove(tabItem);
				}
			}
			else
			{
				await _dialogCoordinator.ShowMessageAsync(this, "Rename key faild", "");
			}
		}

		private async Task KeyValueDeleteKeyCommandActionAsync(RedisDbKeyModel key)
		{
			var id = key.KeyId;

			var dialog = await _dialogCoordinator.ShowMessageAsync(this, "Action confirm", "Are you sure delete the key?", MessageDialogStyle.AffirmativeAndNegative);

			if (dialog == MessageDialogResult.Affirmative)
			{
				var result = await _redisService.KeyDeleteAsync(key);
				if (result)
				{
					key.Remove();

					// if in tabitem 
					var tabItem = this.Tabs.FirstOrDefault(t => t.Id.StartsWith(key.RedisDb.RedisConnect.Guid.ToString()));

					if (tabItem != null)
					{
						this.Tabs.Remove(tabItem);
					}
				}
				else
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Delete key faild", "");
				}
			}
		}

		private async Task KeyValueUpdateTTLCommandActionAsync(RedisDbKeyModel key)
		{
			var id = key.KeyId;
			var tabItem = this.Tabs.FirstOrDefault(t => t.Id.StartsWith(key.RedisDb.RedisConnect.Guid.ToString()));

			if (tabItem == null)
			{
				await _dialogCoordinator.ShowMessageAsync(this, "The key not found", "");
				return;
			}

			var input = await _dialogCoordinator.ShowInputAsync(this, "Set new TTL", "", new MetroDialogSettings() { DefaultText = tabItem.KeyValue.TTL.ToString(), });

			if (string.IsNullOrWhiteSpace(input))
			{
				return;
			}

			if (int.TryParse(input, out int newTTL))
			{
				await _redisService.SetKeyExpireAsync(key, (newTTL <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(newTTL)));

				await KeyValueReloadCommandAsync(key);
			}
		}

		private async Task KeyValueUpdateValueCommandActionAsync(RedisDbKeyModel key, string newText)
		{
			await _redisService.StringSetAsync(key, newText);

			await KeyValueReloadCommandAsync(key);
		}

		private Task KeyCopyCommandExecuteAsync(RedisDbKeyModel key)
		{
			Clipboard.SetText(key.KeyPrefix);

			return Task.CompletedTask;
		}

		private async Task KeyDeleteCommandExecuteAsync(RedisDbKeyModel key)
		{
			if (key.Deleted) return;

			await KeyValueDeleteKeyCommandActionAsync(key);
		}

		private void RemoveTabItems(Guid guid)
		{
			var tabs = this.Tabs.Where(t => t.Id.StartsWith(guid.ToString())).ToList();

			foreach (var item in tabs)
			{
				this.Tabs.Remove(item);
			}
		}
	}
}
