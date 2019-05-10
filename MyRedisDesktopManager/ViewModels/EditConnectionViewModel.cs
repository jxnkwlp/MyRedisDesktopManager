using MahApps.Metro.Controls.Dialogs;
using MyRedisDesktopManager.Services;
using MyRedisDesktopManager.Validators;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyRedisDesktopManager.ViewModels
{
	public class EditConnectionViewModel : ViewModelBase, IDataErrorInfo
	{
		private readonly SettingsService _settingsService = new SettingsService();
		private readonly RedisService _redisService = new RedisService();

		private AddConnectionValidator _validations = new AddConnectionValidator();

		private readonly IDialogCoordinator _dialogCoordinator;
		private string _name;
		private string _host;
		private int _port = 6379;
		private string _password;
		private int _timeout = 30;
		private string _keySeparator;

		public ICommand CloseCommand => new Command((s) => Close?.Invoke());

		public ICommand SaveCommand => new Command((s) => Save());

		public ICommand TestCommand => new Command(async (s) => await TestConnectionAsync());


		/// <summary>
		///  关闭
		/// </summary>
		public Action Close { get; set; }


		public Guid Guid { get; set; } = Guid.Empty;

		public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

		public string Host { get => _host; set { _host = value; OnPropertyChanged(); } }

		public int Port { get => _port; set { _port = value; OnPropertyChanged(); } }

		public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

		public int Timeout { get => _timeout; set { _timeout = value; OnPropertyChanged(); } }

		public string KeySeparator { get => _keySeparator; set => _keySeparator = value; }

		public string Error => string.Empty;

		public string this[string columnName]
		{
			get
			{
				var result = _validations.ValidateFields(this, columnName);

				if (!result.IsValid)
				{
					return result.Errors.Select(t => t.ErrorMessage).FirstOrDefault();
				}

				return null;
			}
		}

		public EditConnectionViewModel()
		{
			_dialogCoordinator = DialogCoordinator.Instance;
		}

		private void Save()
		{
			var validateResult = new AddConnectionValidator().Validate(this);
			if (!validateResult.IsValid)
			{
				Validator.ValidateObject(this, new ValidationContext(this, null, null), true);
				return;
			}

			if (this.Guid == Guid.Empty)
			{
				_settingsService.Add(new Models.ConnectionSettingModel()
				{
					Guid = Guid.NewGuid(),
					Name = this.Name,
					Host = this.Host,
					Port = this.Port,
					Password = this.Password,
					Timeout = this.Timeout,
				});
			}
			else
			{
				_settingsService.Update(new Models.ConnectionSettingModel()
				{
					Guid = this.Guid,
					Name = this.Name,
					Host = this.Host,
					Port = this.Port,
					Password = this.Password,
					Timeout = this.Timeout,
				});
			}

			this.Close?.Invoke();
		}

		private async Task TestConnectionAsync()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			CancellationToken ct = cancellationTokenSource.Token;

			var validateResult = new AddConnectionValidator().Validate(this);
			if (!validateResult.IsValid)
			{
				Validator.ValidateObject(this, new ValidationContext(this, null, null), true);
				return;
			}

			var progress = await _dialogCoordinator.ShowProgressAsync(this, "Test connection", "connection ...", true, new MetroDialogSettings() { CancellationToken = ct, });

			progress.Canceled += async (sender, e) =>
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					cancellationTokenSource.Cancel(false);
				}

				if (progress.IsOpen)
					await progress.CloseAsync();
			};


			try
			{
				progress.SetIndeterminate();

				await Task.Delay(TimeSpan.FromSeconds(0.3));

				await Task.Run(async () =>
				{
					var result = await _redisService.TestConnectionAsync(new Models.ConnectionSettingModel()
					{
						Name = this.Name,
						Host = this.Host,
						Port = this.Port,
						Password = this.Password,
						Timeout = this.Timeout,
					});

					if (result)
					{
						await progress.CloseAsync().ContinueWith(async (t) =>
						{
							await _dialogCoordinator.ShowMessageAsync(this, "Test connection", "Connection success.");
						});
					}
					else
					{
						await progress.CloseAsync().ContinueWith(async (t) =>
						{
							await _dialogCoordinator.ShowMessageAsync(this, "Test connection", "");
						});
					}
				});
			}
			catch (Exception ex)
			{
				//progress.SetMessage(ex.Message);
				await progress.CloseAsync().ContinueWith(async (t) =>
				{
					await _dialogCoordinator.ShowMessageAsync(this, "Test connection", ex.Message);
				});
			}
		}

	}
}
