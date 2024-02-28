using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MotorcycleMAUI.ViewModel
{
	public class FieldViewModel : ViewModelBase
	{
		private Color _color;

		public Color Color { get => _color; set { _color = value; OnPropertyChanged(); } }
		public int RowIndex { get; set; }
		public int ColumnIndex { get; set; }

		public FieldViewModel(Color color, int rowIndex, int columnIndex)
		{
			_color = color;
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
		}
	}
}
