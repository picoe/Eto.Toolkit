using System;
using System.Reflection;
using Eto.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eto.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.ComponentModel;
using Eto.UnitTest.Runners;
using Eto.UnitTest.Filters;

namespace Eto.UnitTest.UI
{
    public class UnitTestPanel : Panel, INotifyPropertyChanged
	{
        ITestRunner _runner;
        TreeGridView _tree;
		Button _startButton;
		Button _stopButton;
		Control _filterControls;
		SearchBox _search;
		TextArea _log;
		UnitTestProgressBar _progress;
		Label _testCountLabel;
		UITimer _timer;
		Panel _customFilterControls;
		ITestFilter _customFilter;
		Dictionary<object, UnitTestItem> _testMap;
		Dictionary<TestStatus, Image> _stateImages = new Dictionary<TestStatus, Image>();
		Image _notRunStateImage;
		Image _runningStateImage;
		ConcurrentDictionary<ITest, ITestResult> _lastResultMap = new ConcurrentDictionary<ITest, ITestResult>();
		ConcurrentDictionary<ITest, IList<ITestResult>> _allResultsMap = new ConcurrentDictionary<ITest, IList<ITestResult>>();
		AsyncQueue _asyncQueue = new AsyncQueue();
		IEnumerable<ITestFilter> _statusFilters = Enumerable.Empty<ITestFilter>();
		IEnumerable<string> _includeCategories = Enumerable.Empty<string>();
		IEnumerable<string> _excludeCategories = Enumerable.Empty<string>();
		IList<string> _availableCategories;

		public event EventHandler<UnitTestLogEventArgs> Log;
		public event PropertyChangedEventHandler PropertyChanged;

		public new Control Content
		{
			get => _customFilterControls.Content;
			set => _customFilterControls.Content = value;
		}

		public ITestFilter CustomFilter
		{
			get => _customFilter;
			set
			{
				_customFilter = value;
				if (Loaded)
					PopulateTree();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to merge nodes with only a single child into its parent.
		/// </summary>
		/// <value><c>true</c> to merge single nodes; otherwise, <c>false</c>.</value>
		public bool MergeSingleNodes { get; set; } = true;

		public ITestRunner Runner
        {
            get => _runner;
            set
            {
                if (_runner != null)
                {
                    _runner.Log -= Runner_Log;
                    _runner.Progress -= Runner_Progress;
                    _runner.TestFinished -= Runner_TestFinished;
                    _runner.TestStarted -= Runner_TestStarted;
                    _runner.IsRunningChanged -= Runner_IsRunningChanged;
                }
                _runner = value;
                if (_runner != null)
                {
                    _runner.Log += Runner_Log;
                    _runner.Progress += Runner_Progress;
                    _runner.TestFinished += Runner_TestFinished;
                    _runner.TestStarted += Runner_TestStarted;
                    _runner.IsRunningChanged += Runner_IsRunningChanged;
                    base.Content.DataContext = _runner;
                }
                PopulateTree();

            }
        }

		IEnumerable<ITestFilter> GetOptionalFilters()
		{
			foreach (var value in Enum.GetValues(typeof(TestStatus)).Cast<TestStatus>())
			{
				yield return new StatusFilter(LookupResult, value);
			}

			yield return new NotRunFilter(LookupResult);
		}

		ITestResult LookupResult(ITest test)
		{
			if (!_lastResultMap.TryGetValue(test, out var result))
				return null;
			return result;
		}

		IEnumerable<ITestFilter> StatusFilters
		{
			get => _statusFilters;
			set
			{
				_statusFilters = value.ToList();
				PopulateTree();
			}
		}

		IEnumerable<string> IncludeCategories
		{
			get => _includeCategories;
			set
			{
				_includeCategories = value.ToList();
				PopulateTree();
			}
		}

		IEnumerable<string> ExcludeCategories
		{
			get => _excludeCategories;
			set
			{
				_excludeCategories = value.ToList();
				PopulateTree();
			}
		}

		IEnumerable<string> AvailableCategories
		{
			get => _availableCategories;
			set
			{
				if (ReferenceEquals(value, _availableCategories))
					return;
				var newCategories = value?.ToList();
				if (newCategories == null || _availableCategories?.SequenceEqual(newCategories) != true)
				{
					_availableCategories = newCategories;
					OnPropertyChanged(nameof(AvailableCategories));
					OnPropertyChanged(nameof(HasCategories));
				}
			}
		}

		bool HasCategories => _availableCategories?.Count > 0;

		void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


		public UnitTestPanel(bool showLog = true)
		{
			_customFilterControls = new Panel();

			_progress = new UnitTestProgressBar();

			_timer = new UITimer();
			_timer.Interval = 0.5;
			_timer.Elapsed += (sender, e) => PerformSearch();

			_testCountLabel = new Label {
				VerticalAlignment = VerticalAlignment.Center
			};

			_startButton = new Button { Text = "Start" };
			_startButton.Click += async (s, e) => await RunTests();
            _startButton.BindDataContext(c => c.Enabled, Binding.Property((ITestRunner r) => r.IsRunning).ToBool(false, true, false), DualBindingMode.OneWay);

            _stopButton = new Button { Text = "Stop", Enabled = false };
			_stopButton.Click += (s, e) => Runner?.StopTests();

			_search = new SearchBox();
			_search.PlaceholderText = "Filter(s)";
			_search.Focus();
			_search.KeyDown += (sender, e) =>
			{
				if (e.KeyData == Keys.Enter)
				{
					_startButton.PerformClick();
					e.Handled = true;
				}
			};
			_search.TextChanged += (sender, e) => _timer.Start();


			_tree = new TreeGridView { ShowHeader = false, Size = new Size(400, -1) };
			_tree.Columns.Add(new GridColumn
			{
				DataCell = new ImageTextCell
				{
					TextBinding = Binding.Property((UnitTestItem m) => m.Text),
					ImageBinding = Binding.Property((UnitTestItem m) => m.Image)
				}
			});

			_tree.Activated += async (sender, e) =>
			{
				if (Runner.IsRunning)
					return;
				var item = (UnitTestItem)_tree.SelectedItem;
				if (item != null)
				{
					var filter = item.Filter;
					if (filter != null)
					{
						await RunTests(filter);
					}
				}
			};

			var showOutputCheckBox = new CheckBox { Text = "Show Output" };
			showOutputCheckBox.CheckedBinding.BindDataContext((LoggingTestRunner r) => r.ShowOutput);
            showOutputCheckBox.BindDataContext(r => r.Visible, Binding.Delegate((LoggingTestRunner r) => r != null));

			var buttons = new TableLayout
			{
				Padding = new Padding(10, 0),
				Spacing = new Size(5, 5),
				Rows = { new TableRow(_startButton, _stopButton, showOutputCheckBox, null, _testCountLabel) }
			};

			var statusChecks = new CheckBoxList
			{
				Spacing = new Size(2, 2),
				Orientation = Orientation.Horizontal,
				DataStore = GetOptionalFilters()
			};
			statusChecks.SelectedValuesBinding.CastItems().To<ITestFilter>().Bind(this, c => c.StatusFilters);

			var includeChecks = new CheckBoxList
			{
				Spacing = new Size(2, 2),
			};
			includeChecks.Bind(c => c.DataStore, this, c => c.AvailableCategories);
			includeChecks.SelectedValuesBinding.CastItems().To<string>().Bind(this, c => c.IncludeCategories);
			includeChecks.Bind(c => c.Visible, this, c => c.HasCategories);

			var includeLabel = new Label { Text = "Include" };
			includeLabel.Bind(c => c.Visible, this, c => c.HasCategories);

			var excludeChecks = new CheckBoxList
			{
				Spacing = new Size(2, 2),
			};
			excludeChecks.Bind(c => c.DataStore, this, c => c.AvailableCategories);
			excludeChecks.SelectedValuesBinding.CastItems().To<string>().Bind(this, c => c.ExcludeCategories);
			excludeChecks.Bind(c => c.Visible, this, c => c.HasCategories);

			var excludeLabel = new Label { Text = "Exclude" };
			excludeLabel.Bind(c => c.Visible, this, c => c.HasCategories);

			_filterControls = new TableLayout
			{
				Spacing = new Size(5, 5),
				Rows = {
					new TableRow("Show", statusChecks, null),
					new TableRow(includeLabel, includeChecks),
					new TableRow(excludeLabel, excludeChecks)
				}
			};

			var allFilters = new Panel
			{
				Padding = new Padding(10, 0),
				Content = new Scrollable
				{
					Border = BorderType.None,
					Content = new TableLayout { Rows = { _filterControls, _customFilterControls } }
				}
			};

			if (showLog)
			{
				Size = new Size(950, 600);
				_log = new TextArea { Size = new Size(400, 300), ReadOnly = true, Wrap = false };

				base.Content = new Splitter
				{
					FixedPanel = SplitterFixedPanel.None,

					Panel1 = new TableLayout
					{
						Padding = new Padding(0, 10, 0, 0),
						Spacing = new Size(5, 5),
						Rows = { allFilters, _search, _tree }
					},

					Panel2 = new TableLayout
					{
						Padding = new Padding(0, 10, 0, 0),
						Spacing = new Size(5, 5),
						Rows = { buttons, _progress, _log }
					}
				};
			}
			else
			{
				Size = new Size(400, 400);
				base.Content = new TableLayout
				{
					Padding = new Padding(0, 10, 0, 0),
					Spacing = new Size(5, 5),
					Rows = { buttons, allFilters, _search, _progress, _tree }
				};
			}
		}

		private void PerformSearch()
		{
			_timer.Stop();
			PopulateTree();
		}

		List<UnitTestLogEventArgs> logQueue = new List<UnitTestLogEventArgs>();

		void Runner_IsRunningChanged(object sender, EventArgs e)
		{
			var running = Runner.IsRunning;
			Application.Instance.Invoke(() =>
			{
				_startButton.Enabled = !running;
				_stopButton.Enabled = running;
				_search.ReadOnly = running;
				_filterControls.Enabled = !running;

				if (!running && StatusFilters.Any())
					PopulateTree();
			});
		}

		void Runner_Log(object sender, UnitTestLogEventArgs e)
		{
			lock (logQueue)
			{
				logQueue.Add(e);
			}
			_asyncQueue.Add("log", () =>
			{
				List<UnitTestLogEventArgs> logQueueCopy;
				lock (logQueue)
				{
					logQueueCopy = logQueue;
					logQueue = new List<UnitTestLogEventArgs>();
				}
				var sb = new StringBuilder();

				foreach (var logEvent in logQueueCopy)
				{
					if (_log != null)
					{
						sb.AppendLine(logEvent.Message);
					}
					Log?.Invoke(this, logEvent);
				}

				_log?.Append(sb.ToString(), true);
			});
		}

        void WriteLog(string text) => _log?.Append(text + "\n", true);

		void Runner_Progress(object sender, UnitTestProgressEventArgs e)
		{
			var progressAmount = e.TestCaseCount > 0 ? (float)e.CompletedCount / e.TestCaseCount : 0;
			var color = e.FailCount > 0 ? Colors.Red : e.WarningCount > 0 ? Colors.Yellow : Colors.Green;
			_asyncQueue.Add("progress", () =>
			{
				_progress.Progress = progressAmount;
				_progress.Color = color;
			});
		}


		void Runner_TestStarted(object sender, UnitTestTestEventArgs e)
		{
			var test = e.Test;
			if (_testMap.TryGetValue(test, out var treeItem))
			{
				if (_lastResultMap.ContainsKey(test))
					_lastResultMap.TryRemove(test, out var result);
				_asyncQueue.Add(() =>
				{
					treeItem.Image = RunningStateImage;
					_tree.ReloadItem(treeItem, false);
				});
			}
		}

		IList<ITestResult> GetAllResults(ITest test)
		{
			if (_allResultsMap.TryGetValue(test, out var list))
				return list;

			list = new List<ITestResult>();
			if (_allResultsMap.TryAdd(test, list))
				return list;

			if (_allResultsMap.TryGetValue(test, out list))
				return list;

			throw new InvalidOperationException($"All results does not have an entry for {test.FullName}");
		}



		void Runner_TestFinished(object sender, UnitTestResultEventArgs e)
		{
			var test = e.Result.Test;
			var result = e.Result;
			if (_testMap.TryGetValue(test, out var treeItem))
			{
				_lastResultMap[test] = result;
				GetAllResults(test).Add(result);

				_asyncQueue.Add(() =>
				{
                    treeItem.Image = GetStateImage(result);
					_tree.ReloadItem(treeItem, false);
				});
			}
		}

		Image NotRunStateImage => _notRunStateImage ?? (_notRunStateImage = CreateImage(Colors.Silver, Colors.Black, null));

		Image RunningStateImage => _runningStateImage ?? (_runningStateImage = CreateImage(Colors.Blue, Colors.White, "↻"));

		Image GetStateImage(ITestResult result) => result != null ? GetStateImage(result.Status) : NotRunStateImage;

		Image GetStateImage(TestStatus status)
		{
			if (_stateImages.TryGetValue(status, out var image))
				return image;
			image = CreateImage(status);
			_stateImages[status] = image;
			return image;
		}


		Image CreateImage(TestStatus status)
		{
			switch (status)
			{
				case TestStatus.Warning:
					return CreateImage(Colors.Yellow, Colors.Black, "!");
				case TestStatus.Failed:
					return CreateImage(Colors.Red, (g, b) =>
					{
						var offset = 10;
						var pen = new Pen(Colors.White, 4);
						g.DrawLine(pen, offset, offset, b.Width - offset, b.Height - offset);
						g.DrawLine(pen, b.Width - offset, offset, offset, b.Height - offset);
					});
				case TestStatus.Inconclusive:
					return CreateImage(Colors.Yellow, new Color(Colors.Black, 0.8f), "?");
				case TestStatus.Skipped:
                    return CreateImage(Colors.Yellow, new Color(Colors.Black, 0.8f), "⤸");
				case TestStatus.Passed:
					return CreateImage(Colors.Green, Colors.White, "✓");
                //case TestStatus.Error:
                //    return CreateImage(Colors.Red, Colors.White, "!");
                default:
					throw new NotSupportedException();
			}
		}

		static Image CreateImage(Color color, Action<Graphics, Bitmap> draw)
		{
			var bmp = new Bitmap(32, 32, PixelFormat.Format32bppRgba);
			using (var g = new Graphics(bmp))
			{
				var r = new RectangleF(Point.Empty, bmp.Size);
				r.Inflate(-1, -1);
				g.FillEllipse(color, r);
				draw?.Invoke(g, bmp);
			}
			return bmp.WithSize(16, 16);
		}

		static Image CreateImage(Color color, Color textcolor, string text)
		{
			return CreateImage(color, (g, b) =>
			{
				var r = new RectangleF(Point.Empty, b.Size);
				r.Inflate(-1, -1);
				if (text != null)
				{
					var font = SystemFonts.Default(SystemFonts.Default().Size * 2);
					var size = g.MeasureString(font, text);
                    var location = r.Location + (PointF)(r.Size - size) / 2;
                    g.DrawText(font, textcolor, location, text);
				}
			});
		}

		async Task RunTests(ITestFilter filter = null)
		{
			if (!_startButton.Enabled)
				return;
			_startButton.Enabled = false;
			_progress.Progress = 0;
			_progress.Color = Colors.Green;
			if (_log != null)
				_log.Text = string.Empty;

            // run asynchronously so UI is responsive
            await Runner.RunAsync(CreateFilter(filter));
        }

        ITestFilter CreateFilter(ITestFilter testFilter = null)
		{
			var filters = GetFilters(testFilter).ToList();
			if (filters.Count > 1)
				return new AndFilter(filters);

			if (filters.Count == 0)
				return TestFilter.Empty;

			return filters[0];
		}

		IEnumerable<ITestFilter> GetFilters(ITestFilter testFilter)
		{
			if (_customFilter != null)
				yield return _customFilter;

			if (IncludeCategories.Any())
				yield return new CategoryFilter(IncludeCategories);

			if (ExcludeCategories.Any())
				yield return new NotFilter(new CategoryFilter(ExcludeCategories) { ChildCanMatch = false });

			if (testFilter != null)
				yield return testFilter;

			if (StatusFilters.Any())
				yield return new OrFilter(StatusFilters.OfType<ITestFilter>());

			if (!string.IsNullOrWhiteSpace(_search.Text))
				yield return new KeywordFilter { Keywords = _search.Text };
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			try
			{
				if (_tree != null)
					PopulateTree();
			}
			catch (Exception ex)
			{
				_log?.Append($"Error populating tree\n{ex}", true);
			}
		}

		public void Refresh()
		{
			PopulateTree();
		}

        void PopulateTree()
        {
            if (Runner == null)
            {
                Application.Instance.Invoke(() =>
                {
                    AvailableCategories = null;
                    _testCountLabel.Text = null;
                    _testMap = null;
                    _tree.DataStore = null;
                });
                return;
            }
            var filter = CreateFilter();
            var categories = AvailableCategories;
            Task.Run(async () =>
            {
                var runner = Runner;
                if (runner == null)
                    return;
                var tests = runner.TestSuite;
                if (tests == null)
                    return;

                var map = new Dictionary<object, UnitTestItem>();

                // always show all categories
                if (categories == null)
                    categories = (await runner.GetCategories(TestFilter.Empty)).OrderBy(r => r).ToList();
                var totalTestCount = await runner.GetTestCount(filter);
                var treeNode = ToTree(tests.Assembly, tests, filter, map);
                if (treeNode?.Text != null)
                    treeNode = new UnitTestItem { Children = { treeNode } };
                Application.Instance.AsyncInvoke(() =>
                {
                    AvailableCategories = categories;
                    _testCountLabel.Text = $"{totalTestCount} Tests";
                    _testMap = map;
                    _tree.DataStore = treeNode;
                });
            }).ContinueWith( t =>
            {
                var errorText = $"Exception: {t.Exception}";
                Console.WriteLine(errorText);
                Application.Instance.Invoke(() => _log?.Append(errorText, true));
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        

		class UnitTestItem : TreeGridItem
		{
			public string Text { get; set; }
			public ITest Test { get; set; }
			public Image Image { get; set; }
			public ITestFilter Filter { get; set; }
		}

        UnitTestItem ToTree(Assembly assembly, ITest test, ITestFilter filter, IDictionary<object, UnitTestItem> map)
		{
			// add a test
			var name = test.Name;
            if (test.IsAssembly)
			{
				var an = new AssemblyName(Path.GetFileNameWithoutExtension(test.Name));
				name = an.Name;
			}

			if (!filter.Pass(test))
				return null;

			_lastResultMap.TryGetValue(test, out var result);
			var worstChildResult = test.GetChildren()
				.Select(t => _lastResultMap.TryGetValue(t, out var r) ? r : null)
				.Where(r => r != null)
				.OrderByDescending(r => r.Status)
				.FirstOrDefault();
			if (worstChildResult?.Status > result?.Status)
				result = worstChildResult;

			var item = new UnitTestItem
			{
				Text = name,
				Test = test,
				Image = GetStateImage(result),
				Filter = new SingleTestFilter { Test = test, Assembly = assembly }
			};
			map[test] = item;
			if (test.HasChildren)
			{
                item.Expanded = !test.IsParameterized;
				foreach (var child in test.Tests)
				{
					var treeItem = ToTree(assembly, child, filter, map);
					if (treeItem != null)
						item.Children.Add(treeItem);
				}
				if (MergeSingleNodes && item.Text != null)
				{
					while (item.Children.Count == 1)
					{
						// collapse test nodes
						var child = item.Children[0] as UnitTestItem;
						if (child.Children.Count == 0)
							break;
						if (!child.Text.StartsWith(item.Text, StringComparison.Ordinal))
						{
							var separator = test.IsAssembly ? ":" : ".";

							child.Text = $"{item.Text}{separator}{child.Text}";
						}
						child.Expanded |= test.IsAssembly;
						item = child;
					}
				}
				if (item.Children.Count == 0)
					return null;
			}
			return item;
		}
	}
}
