#if UNITY_EDITOR

using System;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class UsagesFinderSettings
	{
		// ----------------------------------------------------------------------------
		// filtering
		// ----------------------------------------------------------------------------

		/* ignores and includes */

		public string[] pathIgnores = new string[0];
		public string[] pathIncludes = new string[0];

		public int filtersTabIndex = 0;

		public UsagesFinderSettings()
		{
			Reset();
		}

		internal void Reset()
		{
			
		}
	}
}

#endif