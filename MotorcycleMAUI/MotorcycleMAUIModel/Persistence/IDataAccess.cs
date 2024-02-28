using MotorcycleMAUIModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MotorcycleMAUIModel.Model.States;

namespace MotorcycleMAUIModel.Persistence
{
	public interface IDataAccess
	{
		Task<(int, int, int, int, FieldState[,])> LoadGameAsync(string path);
		public Task<bool> SaveGameAsync(string path, MotorcycleModel model);
	}
}
