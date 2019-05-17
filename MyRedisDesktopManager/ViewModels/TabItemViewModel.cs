namespace MyRedisDesktopManager.ViewModels
{
	public class TabItemViewModel : ViewModelBase
	{
		private string _title;

		public string Id { get; set; }

		public string Title { get => _title; set { _title = value; OnPropertyChanged(nameof(Title)); } }


	}
}
