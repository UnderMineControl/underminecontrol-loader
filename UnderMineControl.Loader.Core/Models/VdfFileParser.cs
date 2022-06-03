using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnderMineControl.Loader.Core
{
	public static class Extensions
	{
		public static string GetBetween(this string str, char begin, char end)
		{
			var beginIndex = str.IndexOf(begin);
			var endIndex = str.LastIndexOf(end);

			return str.Substring(beginIndex + 1, endIndex - beginIndex - 1);
		}
	}

	public sealed class VdfFileParser
	{
		private readonly string _lines;

		public VdfFileParser(string lines)
		{
			_lines = lines;
		}

		public Dictionary<string, VdfFileFolderData> Parse()
		{
			Dictionary<string, VdfFileFolderData> _folderDatas = new Dictionary<string, VdfFileFolderData>();

			var allFolderData = _lines.GetBetween('{', '}');
			var lines = allFolderData.Split('\n').ToList();

			var startIndex = -1;
			var currentId = -1;

			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];

				if (int.TryParse(line.Trim().Trim('"'), out var val))
				{
					currentId = val;
					continue;
				}

				if (line.StartsWith("\t{"))
				{
					startIndex = i;
				}

				if (line.StartsWith("\t}"))
				{
					_folderDatas.Add(currentId.ToString(), ParseObject(startIndex + 1, i, lines));
				}
			}

			return _folderDatas;
		}

		private static VdfFileFolderData ParseObject(int startIndex, int endIndex, List<string> lines)
		{
			// print lines
			//Console.WriteLine($"------------ {folderId}");

			var data = new VdfFileFolderData();

			var insideApps = false;

			for (var i = startIndex; i < endIndex; i++)
			{
				var line = lines[i].Trim();

				switch (line)
				{
					case "\"apps\"":
						insideApps = true;
						i++;
						continue;
					case "}":
						insideApps = false;
						continue;
				}

				var splitted = line.Split('\t');
				var key = splitted[0].Trim('"');
				var value = splitted[2].Trim('"');

				if (!insideApps)
				{
					//Console.WriteLine($"{key} : {value}");

					switch (key)
					{
						case "path":
							data.Path = value;
							break;
						case "label":
							data.Label = value;
							break;
						case "contentid":
							data.ContentId = ulong.Parse(value);
							break;
						case "totalsize":
							data.TotalSize = ulong.Parse(value);
							break;
						case "update_clean_bytes_tally":
							data.UpdateCleanBytesTally = ulong.Parse(value);
							break;
						case "time_last_update_corruption":
							data.TimeLastUpdateCorruption = ulong.Parse(value);
							break;
					}
				}
				else
				{
					data.Apps.Add(key, value);
				}
			}

			return data;
			//Console.WriteLine("------------");
		}

	}

	public sealed class VdfFileFolderData
	{
		public string Path { get; set; }
		public string Label { get; set; }
		public ulong ContentId { get; set; }
		public ulong TotalSize { get; set; }
		public ulong UpdateCleanBytesTally { get; set; }
		public ulong TimeLastUpdateCorruption { get; set; }
		public Dictionary<string, string> Apps => new Dictionary<string, string>();

	}
}