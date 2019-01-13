#if UNITY_EDITOR

using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class UsagesFiltersWindow : FiltersWindow
	{
		internal static UsagesFiltersWindow instance;

		internal static UsagesFiltersWindow Create()
		{
			UsagesFiltersWindow window = GetWindow<UsagesFiltersWindow>(true);
			window.Focus();

			return window;
		}

		internal static void Refresh()
		{
			if (instance == null) return;

			instance.InitOnEnable();
			instance.Focus();
		}

		protected override void InitOnEnable()
		{
			TabBase[] tabs =
			{
				new PathFiltersTab(FilterType.Includes, MaintainerSettings.Usages.pathIncludes, true, OnPathIncludesChange),
				new PathFiltersTab(FilterType.Ignores, MaintainerSettings.Usages.pathIgnores, true, OnPathIgnoresChange)
			};

			Init(IssuesFinder.MODULE_NAME, tabs, MaintainerSettings.Usages.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private void OnPathIgnoresChange(string[] collection)
		{
			MaintainerSettings.Usages.pathIgnores = collection;
		}

		private void OnPathIncludesChange(string[] collection)
		{
			MaintainerSettings.Usages.pathIncludes = collection;
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Usages.filtersTabIndex = newTab;
		}
	}
}
#endif