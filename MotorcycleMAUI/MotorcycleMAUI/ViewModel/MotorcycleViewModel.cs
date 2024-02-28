using System;
using MotorcycleMAUIModel.Model;
using static MotorcycleMAUIModel.Model.States;
using MotorcycleMAUIModel.EventArguments;
using MotorcycleMAUIModel.Persistence;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MotorcycleMAUI.ViewModel
{
	public class MotorcycleViewModel : ViewModelBase
	{

		#region Fields

		private MotorcycleModel _model;
		private bool _pause;
		private bool _start;
		private IDispatcherTimer _timer;

		#endregion

		#region Properties

		public int Size { get => _model.Size; set { OnPropertyChanged(); } }
		public int FuelTank { get => _model.FuelTank; set { OnPropertyChanged(); } }
		public string Time { get => _model.IntToTime(_model.Time); set { OnPropertyChanged(); } }
		public bool Pause { get => _pause; set { _pause = value; OnPropertyChanged(); } }
		public bool Start { get => _start; set { _start = value; OnPropertyChanged(); } }
		#endregion

		#region ObservableCollections
		public ObservableCollection<FieldViewModel> Fields { get; set; }

		public RowDefinitionCollection GameTableRows
		{
			get => new RowDefinitionCollection(Enumerable.Repeat(new RowDefinition(GridLength.Star), Size).ToArray());
		}

		public ColumnDefinitionCollection GameTableColumns
		{
			get => new ColumnDefinitionCollection(Enumerable.Repeat(new ColumnDefinition(GridLength.Star), Size).ToArray());
		}


		#endregion

		#region DelegateCommands
		public DelegateCommand MoveMotorCommand { get; set; }
		public DelegateCommand NewGameCommand { get; set; }
		public DelegateCommand StartCommand { get; set; }
		public DelegateCommand PauseCommand { get; set; }
		public DelegateCommand SaveGameCommand { get; set; }
		public DelegateCommand LoadGameCommand { get; set; }

		public DelegateCommand ExitCommand { get; private set; }
		#endregion

		#region EventHandlers
		public event EventHandler? LoadGame;
		public event EventHandler? SaveGame;
		public event EventHandler? NewGame;
		public event EventHandler? ExitGame;
		#endregion
		public MotorcycleViewModel(MotorcycleModel model)
		{
			_model = model;
			_model.GameStarted += OnGameStarted;
			_model.FieldChanged += OnFieldChanged;
			_model.GameOver += OnGameFinished;

			while (Application.Current is null) ;

			_timer = Application.Current.Dispatcher.CreateTimer();
			_timer.Interval = TimeSpan.FromMilliseconds(1000);
			_timer.Tick += OnTimerTicked;

			Fields = new ObservableCollection<FieldViewModel>();

			MoveMotorCommand = new DelegateCommand(param => KeyDown(Convert.ToString(param)));
			StartCommand = new DelegateCommand(param => OnStartClicked());
			PauseCommand = new DelegateCommand(param => OnPauseClicked());
			NewGameCommand = new DelegateCommand(param => OnNewGame());
			SaveGameCommand = new DelegateCommand(param => OnSaveGame());
			LoadGameCommand = new DelegateCommand(param => OnLoadGame());
			ExitCommand = new DelegateCommand(param => OnExitGame());

			_model.StartNewGame(13);

		}

		private void OnExitGame()
		{
			ExitGame?.Invoke(this, EventArgs.Empty);
		}

		private void OnNewGame()
		{
			_model.StartNewGame(13);
			NewGame?.Invoke(this, EventArgs.Empty);
		}

		private void OnStartClicked()
		{
			_timer.Start();
			Pause = true;
			Start = false;
		}
		private void OnPauseClicked()
		{
			_timer.Stop();
			Pause = false;
			Start = true;
		}
		private void KeyDown(string? key)
		{
			if(_timer.IsRunning && key is not null)
			{
				KeyState keyValue = _model.StringToKeyState(key);
				_model.KeyPressed(keyValue);
			}
		}
		private void OnTimerTicked(object? sender, EventArgs e)
		{
			_model.TimerTicked();
			FuelTank = _model.FuelTank;
			Time = _model.IntToTime(_model.Time);
			_timer.Interval = TimeSpan.FromMilliseconds(_model.Speed);
		}
		private void OnGameFinished(object? sender, GameOverEventArgs e)
		{
			_timer.Stop();
			Start = true;
			Pause = false;
		}
		private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
		{
			var field = Fields.FirstOrDefault(field => field.RowIndex == e.Row && field.ColumnIndex == e.Column);
			if (field != null) {
				field.Color = CalculateColor(e.NewState);
			}
		}
		private void OnGameStarted(object? sender, GameStartedEventArgs e)
		{
			_timer.Stop();
			Pause = false;
			Start = true;
			FuelTank = _model.FuelTank;
			Time = _model.IntToTime(_model.Time);
			Fields.Clear();

			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					Fields.Add(new FieldViewModel(CalculateColor(e.Board[i,j]),i,j));
				}
			}

		}
		private Color CalculateColor(FieldState field)
		{
			switch (field)
			{
				case FieldState.Empty:
					return Colors.White;
				case FieldState.Fuel:
					return Colors.Red;
				case FieldState.Motor:
					return Colors.Black;
				default:
					return Colors.Blue;
			}
		}
		private void OnSaveGame()
		{
			SaveGame?.Invoke(this, EventArgs.Empty);
		}
		private void OnLoadGame()
		{
			LoadGame?.Invoke(this, EventArgs.Empty);
		}
	}
}
