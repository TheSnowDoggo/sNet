using System.ComponentModel;
using System.Text;

namespace sNet;

public sealed class ColumnBuilder
{
	private readonly List<List<string>> _rows;

	public ColumnBuilder()
	{
		_rows = [];
	}

	public ColumnBuilder(int rowCapacity)
	{
		_rows = new List<List<string>>(rowCapacity);
	}

	public List<string> this[int index]
	{
		get => _rows[index];
		set => _rows[index] = value;
	}
	
	public int Rows => _rows.Count;
	public int RowCapacity => _rows.Capacity;
	
	public string LeftBorder { get; set; } = "| ";
	public string CenterBorder { get; set; } = " | ";
	public string RightBorder { get; set; } = " |";
	
	public char HorizontalBorder { get; set; } = '-';
	
	public char Fill { get; set; } = ' ';
	public ColumnPadding Padding { get; set; }
	
	public string Prefix { get; set; }
	public string Suffix { get; set; }

	public void AddRow(params List<string> row)
	{
		_rows.Add(row);
	}
	
	public void AddRow(params List<object> objects)
	{
		var row = new List<string>(objects.Count);

		foreach (object obj in objects)
		{
			row.Add(obj?.ToString() ?? string.Empty);
		}
		
		_rows.Add(row);
	}

	public void AddHeader(params List<string> titles)
	{
		AddHorizontalBorder();
		AddRow(titles);
		AddHorizontalBorder();
	}

	public void StartRow(int capacity = 0)
	{
		_rows.Add(new List<string>(capacity));
	}
	
	public void AppendColumn(string column)
	{
		_rows[^1].Add(column);
	}

	public void AddHorizontalBorder()
	{
		_rows.Add(null);
	}

	public void Clear()
	{
		_rows.Clear();
	}

	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.Append(Prefix);

		List<int> widths = GetWidths();
		
		int totalWidth = widths.Sum() + CenterBorder.Length * (widths.Count - 1) + LeftBorder.Length + RightBorder.Length;

		for (int i = 0; i < _rows.Count; i++)
		{
			if (i != 0)
			{
				sb.AppendLine();
			}
			
			List<string> row = _rows[i];
			
			if (row == null)
			{
				sb.Append(HorizontalBorder, totalWidth);
				continue;
			}

			sb.Append(LeftBorder);

			for (int j = 0; j < widths.Count; j++)
			{
				if (j != 0)
				{
					sb.Append(CenterBorder);
				}
				
				int width = widths[j];
				
				if (j >= row.Count)
				{
					sb.Append(Fill, width);
					continue;
				}

				sb.Append(Pad(row[j], width));
			}
			
			sb.Append(RightBorder);
		}
		
		sb.Append(Suffix);
		
		return sb.ToString();
	}

	private string Pad(string str, int width)
	{
		return Padding switch
		{
			ColumnPadding.Left  => str.PadLeft(width),
			ColumnPadding.Right => str.PadRight(width),
			_ => throw new InvalidEnumArgumentException(nameof(Padding), (int)Padding, typeof(ColumnPadding))
		};
	}

	private List<int> GetWidths()
	{
		var widths = new List<int>();

		foreach (List<string> row in _rows)
		{
			if (row == null)
			{
				continue;
			}
			
			for (int i = 0; i < row.Count; i++)
			{
				string column = row[i];
				
				if (i >= widths.Count)
				{
					widths.Add(column.Length);
					continue;
				}

				if (column.Length > widths[i])
				{
					widths[i] = column.Length;
				}
			}
		}

		return widths;
	}
}