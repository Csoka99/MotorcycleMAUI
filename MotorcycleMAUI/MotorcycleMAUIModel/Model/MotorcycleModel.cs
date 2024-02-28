using MotorcycleMAUIModel.EventArguments;
using MotorcycleMAUIModel.Persistence;
using System;
using System.Drawing;
using System.Threading.Tasks;
using static MotorcycleMAUIModel.Model.States;

namespace MotorcycleMAUIModel.Model
{
	public class MotorcycleModel
	{
		#region Field

		private IDataAccess dataAccess;
		private FieldState[,] _board;
		private int _size;
		private int _fuelTank;
		private int _speed;
		private int _time;
		private Random _rnd;
		private bool _isGameOver;

		#endregion

		#region Properties

		public int Size { get =>  _size; }
		public int Time { get => _time; }
		public int FuelTank { get => _fuelTank; }
		public int Speed { get => _speed; }
		public FieldState[,] Board { get => _board; }
		public bool IsGameOver { get => _isGameOver; }

		#endregion

		#region EventArgs

		public event EventHandler<GameStartedEventArgs>? GameStarted;
		public event EventHandler<FieldChangedEventArgs>? FieldChanged;
		public event EventHandler<GameOverEventArgs>? GameOver;

		#endregion
		public MotorcycleModel(IDataAccess dataAccess)
		{
			this.dataAccess = dataAccess;
			_board = new FieldState[0, 0];
			_rnd = new Random();
		}

		public void StartNewGame(int size)
		{
			this._size = size;
			_isGameOver = false;
			_board = new FieldState[size,size];
			_speed = 1000;
			_fuelTank = 50;
			_time = 0;

			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					_board[i, j] = FieldState.Empty;
				}
			}

			_board[size-1, (size-1)/2] = FieldState.Motor;

			if (GameStarted is not null)
			{
				GameStarted(this, new GameStartedEventArgs(size, _board));
			}
		}

		public KeyState StringToKeyState(string key)
		{
			switch(key)
			{
				case "Right":
					return KeyState.Right;
				case "Left":
					return KeyState.Left;
				default:
					return KeyState.Invalid;
			}
		}

		public void TimerTicked()
		{
			if (!IsGameOver)
			{
				_time += _speed;
				_fuelTank--;
				if (_speed > 200)
				{
					if (_speed < 500)
					{
						_speed -= 5;
					}
					else
					{
						_speed -= 10;
					}
				}


				for (int i = _size - 1; i >= 0; i--)
				{
					for (int j = 0; j < _size; j++)
					{
						if (_board[i, j] == FieldState.Fuel)
						{
							if (i + 1 < _size)
							{
								if (_board[i + 1, j] == FieldState.Motor)
								{
									if (_fuelTank + 5 < 50)
									{
										_fuelTank += 5;
									}
									else
									{
										_fuelTank = 50;
									}
								}
								else
								{
									_board[i + 1, j] = FieldState.Fuel;
								}

							}
							_board[i, j] = FieldState.Empty;
						}
					}
				}

				_rnd = new Random();
				int chance = _rnd.Next(0, 1000);
				int fuelRow = 0;
				if (chance < 300)
				{
					fuelRow = _rnd.Next(_size);
					_board[0, fuelRow] = FieldState.Fuel;
				}

				for (int i = 0; i < _size; i++)
				{
					for (int j = 0; j < _size; j++)
					{
						if (FieldChanged is not null)
						{
							FieldChanged(this, new FieldChangedEventArgs(i, j, _board[i, j]));
						}
					}
				}

				if (_fuelTank == 0)
				{
					if (GameOver is not null)
					{
						GameOver(this, new GameOverEventArgs(_time,_isGameOver));
						_isGameOver = true;
					}
				}

			}

		}

		public void KeyPressed(KeyState key)
		{
			if (!IsGameOver)
			{
				if (key == KeyState.Right)
				{
					for (int i = 0; i < _size; i++)
					{
						for (int j = _size - 1; j >= 0; j--)
						{
							if (_board[i, j] == FieldState.Motor && j < _size - 1)
							{
								if (_board[i, j + 1] == FieldState.Fuel)
								{
									if (_fuelTank + 5 < 50)
									{
										_fuelTank += 5;
									}
									else
									{
										_fuelTank = 50;
									}
								}
								_board[i, j + 1] = FieldState.Motor;
								_board[i, j] = FieldState.Empty;
							}
						}
					}
				}
				else if (key == KeyState.Left)
				{
					for (int i = 0; i < _size; i++)
					{
						for (int j = 0; j < _size; j++)
						{
							if (_board[i, j] == FieldState.Motor && j > 0)
							{
								if (_board[i, j - 1] == FieldState.Fuel)
								{
									if (_fuelTank + 5 < 50)
									{
										_fuelTank += 5;
									}
									else
									{
										_fuelTank = 50;
									}
								}
								_board[i, j - 1] = FieldState.Motor;
								_board[i, j] = FieldState.Empty;
							}
						}
					}
				}


				for (int i = 0; i < _size; i++)
				{
					for (int j = 0; j < _size; j++)
					{
						if (FieldChanged is not null)
						{
							FieldChanged(this, new FieldChangedEventArgs(i, j, _board[i, j]));
						}
					}
				}
			}
			
		}

		public string IntToTime(int time)
		{
			int val = time;
			TimeSpan result = TimeSpan.FromMilliseconds(val);
			string fromTimeString = result.ToString("mm':'ss");
			return fromTimeString;
		}

		public async Task SaveGame(string path)
		{
			await dataAccess.SaveGameAsync(path, this);
		}

		public async Task LoadGame(string path)
		{
			(_size, _time, _fuelTank, _speed, _board) = await dataAccess.LoadGameAsync(path);

			for (int i = 0; i < _size; i++)
			{
				for (int j = 0; j < _size; j++)
				{
					if (FieldChanged is not null)
					{
						FieldChanged(this, new FieldChangedEventArgs(i, j, _board[i, j]));
					}
				}
			}
		}
	}
}
