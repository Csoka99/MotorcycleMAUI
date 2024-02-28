using Moq;
using MotorcycleMAUIModel.Model;
using MotorcycleMAUIModel.Persistence;
using static MotorcycleMAUIModel.Model.States;

namespace MotorcycleGameTest
{
	[TestClass]
	public class MotorcycleGameTest
	{

		private MotorcycleModel? model;
		private Mock<IDataAccess>? mock;
		private FieldState[,]? mockedBoard;
		private int mockedTime;
		private int mockedSpeed;
		private int mockedFuelSize;
		private int mockedSize;

		[TestInitialize]
		public void Initialize()
		{
			mockedSize = 13;
			mockedTime = 0;
			mockedSpeed = 1000;
			mockedFuelSize = 50;
			mockedBoard = new FieldState[mockedSize, mockedSize];

			for (int i = 0; i < mockedSize; i++)
			{
				for (int j = 0; j < mockedSize; j++)
				{
					mockedBoard[i, j] = FieldState.Empty;
				}
			}

			mockedBoard[mockedSize - 1, (mockedSize - 1) / 2] = FieldState.Motor;

			mock = new Mock<IDataAccess>();
			mock.Setup(mock => mock.LoadGameAsync(It.IsAny<String>()))
				.Returns(() => Task.FromResult((mockedSize, mockedTime, mockedFuelSize, mockedSpeed, mockedBoard)));

			model = new MotorcycleModel(mock.Object);

		}

		[TestMethod]
		public void MotorcycleModelNewGameTest()
		{
			model?.StartNewGame(mockedSize);

			Assert.AreEqual(model?.Time, mockedTime);
			Assert.AreEqual(model?.FuelTank, mockedFuelSize);
			Assert.AreEqual(model?.Speed, mockedSpeed);

			for (int i = 0; i < mockedSize; i++)
			{
				for (int j = 0; j < mockedSize; j++)
				{
					Assert.AreEqual(model?.Board?[i, j], mockedBoard?[i, j]);
				}
			}
		}

		[TestMethod]
		public void MotorcycleModelStepRightTest()
		{
			model?.StartNewGame(mockedSize);
			model?.KeyPressed(KeyState.Right);

			if (mockedBoard?[12, 6] is not null)
			{
				mockedBoard[12, 6] = FieldState.Empty;
			}

			if (mockedBoard?[12, 7] is not null)
			{
				mockedBoard[12, 7] = FieldState.Motor;
			}
			for (int i = 0; i < mockedSize; i++)
			{
				for (int j = 0; j < mockedSize; j++)
				{
					Assert.AreEqual(model?.Board?[i, j], mockedBoard?[i, j]);
				}
			}
		}

		[TestMethod]
		public void MotorcycleModelStepLeftTest()
		{
			model?.StartNewGame(mockedSize);
			model?.KeyPressed(KeyState.Left);

			if (mockedBoard?[12, 6] is not null)
			{
				mockedBoard[12, 6] = FieldState.Empty;
			}

			if (mockedBoard?[12, 5] is not null)
			{
				mockedBoard[12, 5] = FieldState.Motor;
			}

			for (int i = 0; i < mockedSize; i++)
			{
				for (int j = 0; j < mockedSize; j++)
				{
					Assert.AreEqual(model?.Board?[i, j], mockedBoard?[i, j]);
				}
			}
		}

		[TestMethod]
		public void MotorcycleModelTickedTest()
		{
			model?.StartNewGame(mockedSize);
			model?.TimerTicked();
			model?.TimerTicked();
			model?.TimerTicked();

			for (int i = 0; i < mockedSize; i++)
			{
				if (model?.Board?[0, i] is not null && mockedBoard?[1, i] is not null)
				{
					mockedBoard[1, i] = model.Board[0, i];
				}
			}

			model?.TimerTicked();

			mockedTime = 3940;
			mockedSpeed = 960;
			mockedFuelSize = 46;

			Assert.AreEqual(model?.FuelTank, mockedFuelSize);
			Assert.AreEqual(model?.Time, mockedTime);
			Assert.AreEqual(model?.Speed, mockedSpeed);

			for (int i = 0; i < mockedSize; i++)
			{
				Assert.AreEqual(model?.Board?[1, i], mockedBoard?[1, i]);
			}
		}

		[TestMethod]
		public async Task MotorcycleModelLoadTest()
		{
			if (model is not null && mock is not null)
			{
				model.StartNewGame(mockedSize);

				await model.LoadGame(String.Empty);

				for (int i = 0; i < mockedSize; i++)
				{
					for (int j = 0; j < mockedSize; j++)
					{
						Assert.AreEqual(mockedBoard?[i, j], model.Board?[i, j]);
					}
				}

				mock.Verify(dataAccess => dataAccess.LoadGameAsync(String.Empty), Times.Once());
			}
		}

		[TestMethod]
		public void MotorcycleModelCatchedFuelCellTest()
		{
			model?.StartNewGame(mockedSize);

			bool isFuel = false;
			int where = 0;
			int tickedTime = 0;

			while (!isFuel)
			{
				model?.TimerTicked();
				tickedTime++;
				for (int i = 0; i < mockedSize; i++)
				{
					if (model?.Board?[0, i] == FieldState.Fuel)
					{
						isFuel = true;
						where = i;
					}
				}
			}

			for (int i = 0; i < 11; i++)
			{
				model?.TimerTicked();
			}

			while (model?.Board?[mockedSize - 1, where] != FieldState.Motor)
			{
				for (int i = 0; i < mockedSize; i++)
				{
					if (model?.Board?[mockedSize - 1, i] == FieldState.Motor)
					{
						if (where < i)
						{
							model.KeyPressed(KeyState.Left);
						}
						else if (where > i)
						{
							model.KeyPressed(KeyState.Right);
						}
					}
				}
			}

			model.TimerTicked();
			tickedTime += 12;

			mockedFuelSize = (50 - tickedTime) + 5;

			Assert.AreEqual(model.FuelTank, mockedFuelSize);
		}

		[TestMethod]
		public void Model_GameOver()
		{
			model?.StartNewGame(mockedSize);
			while (model?.FuelTank != 0)
			{
				model?.TimerTicked();
			}
			Assert.AreEqual(0, model?.FuelTank);
		}
	}
}