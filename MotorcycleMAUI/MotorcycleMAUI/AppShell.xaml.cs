using MotorcycleMAUI.View;
using MotorcycleMAUI.ViewModel;
using MotorcycleMAUIModel.EventArguments;
using MotorcycleMAUIModel.Model;
using MotorcycleMAUIModel.Persistence;

namespace MotorcycleMAUI
{
	public partial class AppShell : Shell
	{

		private IDataAccess _dataAccess;
		private readonly MotorcycleModel _gameModel;
		private readonly MotorcycleViewModel _viewModel;


		private readonly IStore _store;
		private readonly StoredGameBrowserModel _storedGameBrowserModel;
		private readonly StoredGameBrowserViewModel _storedGameBrowserViewModel;


		public AppShell(IStore store,
			IDataAccess dataAccess,
			MotorcycleModel gameModel,
			MotorcycleViewModel viewModel)
		{
			InitializeComponent();

			// játék összeállítása
			_store = store;
			_dataAccess = dataAccess;
			_gameModel = gameModel;
			_viewModel = viewModel;

			_gameModel.GameOver += SudokuGameModel_GameOver;
			_viewModel.NewGame += SudokuViewModel_NewGame;
			_viewModel.LoadGame += SudokuViewModel_LoadGame;
			_viewModel.SaveGame += SudokuViewModel_SaveGame;

			// a játékmentések kezelésének összeállítása
			_storedGameBrowserModel = new StoredGameBrowserModel(_store);
			_storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
			_storedGameBrowserViewModel.GameLoading += StoredGameBrowserViewModel_GameLoading;
			_storedGameBrowserViewModel.GameSaving += StoredGameBrowserViewModel_GameSaving;

			_gameModel.StartNewGame(13);
		}


		/// <summary>
		///     Játék végének eseménykezelője.
		/// </summary>
		/// 
		private void SudokuViewModel_NewGame(object? sender, EventArgs e)
		{
			_gameModel.StartNewGame(13);

		}

		private async void SudokuGameModel_GameOver(object? sender, GameOverEventArgs e)
		{
			await DisplayAlert("Motorcycle game", $"Game over! Collapsed time: {_gameModel.IntToTime(e.Time)}", "OK");
		}
		private async void SudokuViewModel_LoadGame(object? sender, EventArgs e)
		{
			await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
			await Navigation.PushAsync(new LoadGamePage
			{
				BindingContext = _storedGameBrowserViewModel
			}); // átnavigálunk a lapra
		}

		/// <summary>
		///     Játék mentésének eseménykezelője.
		/// </summary>
		private async void SudokuViewModel_SaveGame(object? sender, EventArgs e)
		{
			await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
			await Navigation.PushAsync(new SaveGamePage
			{
				BindingContext = _storedGameBrowserViewModel
			}); // átnavigálunk a lapra
		}


		/// <summary>
		///     Betöltés végrehajtásának eseménykezelője.
		/// </summary>
		private async void StoredGameBrowserViewModel_GameLoading(object? sender, StoredGameEventArgs e)
		{
			await Navigation.PopAsync(); // visszanavigálunk

			// betöltjük az elmentett játékot, amennyiben van
			try
			{
				await _gameModel.LoadGame(e.Name);

				// sikeres betöltés
				await Navigation.PopAsync(); // visszanavigálunk a játék táblára
				await DisplayAlert("Motorcycle játék", "Sikeres betöltés.", "OK");

			}
			catch
			{
				await DisplayAlert("Motorcycle játék", "Sikertelen betöltés.", "OK");
			}
		}

		/// <summary>
		///     Mentés végrehajtásának eseménykezelője.
		/// </summary>
		private async void StoredGameBrowserViewModel_GameSaving(object? sender, StoredGameEventArgs e)
		{
			await Navigation.PopAsync(); // visszanavigálunk

			try
			{
				// elmentjük a játékot
				await _gameModel.SaveGame(e.Name);
				await DisplayAlert("Motorcycle játék", "Sikeres mentés.", "OK");
			}
			catch
			{
				await DisplayAlert("Motorcycle játék", "Sikertelen mentés.", "OK");
			}
		}

	}
}
