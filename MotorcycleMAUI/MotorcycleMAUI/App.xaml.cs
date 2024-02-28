using MotorcycleMAUI.Persistence;
using MotorcycleMAUI.ViewModel;
using MotorcycleMAUIModel.Model;
using MotorcycleMAUIModel.Persistence;

namespace MotorcycleMAUI
{
	public partial class App : Application
	{

		private const string SuspendedGameSavePath = "SuspendedGame";

		private readonly AppShell _appShell;
		private readonly IDataAccess _dataAccess;
		private readonly MotorcycleModel _gameModel;
		private readonly IStore _store;
		private readonly MotorcycleViewModel _viewModel;

		public App()
		{
			InitializeComponent();

			_store = new MotorcycleStore();
			_dataAccess = new DataAccess(FileSystem.AppDataDirectory);

			_gameModel = new MotorcycleModel(_dataAccess);
			_viewModel = new MotorcycleViewModel(_gameModel);

			_appShell = new AppShell(_store, _dataAccess, _gameModel, _viewModel)
			{
				BindingContext = _viewModel
			};
			MainPage = _appShell;

		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			Window window = base.CreateWindow(activationState);

			// az alkalmazás indításakor
			window.Created += (s, e) =>
			{
				// új játékot indítunk
				_gameModel.StartNewGame(13);
			};

			// amikor az alkalmazás fókuszba kerül
			window.Activated += (s, e) =>
			{
				if (!File.Exists(Path.Combine(FileSystem.AppDataDirectory, SuspendedGameSavePath)))
					return;

				Task.Run(async () =>
				{
					// betöltjük a felfüggesztett játékot, amennyiben van
					try
					{
						await _gameModel.LoadGame(SuspendedGameSavePath);

					}
					catch
					{
					}
				});
			};

			// amikor az alkalmazás fókuszt veszt
			window.Deactivated += (s, e) =>
			{
				Task.Run(async () =>
				{
					try
					{
						await _gameModel.SaveGame(SuspendedGameSavePath);
					}
					catch
					{
					}
				});
			};

			return window;
		}

	}
}
