using MotorcycleMAUIModel.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorcycleMAUI.Persistence
{
	public class MotorcycleStore : IStore
	{
		public async Task<IEnumerable<String>> GetFilesAsync()
		{
			return await Task.Run(() => Directory.GetFiles(FileSystem.AppDataDirectory)
				.Select(Path.GetFileName)
				.Where(name => name?.EndsWith(".stl") ?? false)
				.OfType<String>());
		}

		public async Task<DateTime> GetModifiedTimeAsync(String name)
		{
			var info = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, name));

			return await Task.Run(() => info.LastWriteTime);
		}
	}
}
