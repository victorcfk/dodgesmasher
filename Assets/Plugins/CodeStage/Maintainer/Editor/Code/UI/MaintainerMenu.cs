#if UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.Usages;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CodeStage.Maintainer.UI
{
	public class MaintainerMenu
	{
		/*[MenuItem("Assets/Maintainer/Find Usages")]
		public static void FindUsages()
		{
			UsagesFinder.StartSearch(CSEditorTools.GetProjectSelectedObjects(false), true);
		}

		[MenuItem("Assets/Maintainer/Find Usages", true)]
		public static bool FindUsagesValidate()
		{
			return CSEditorTools.GetProjectSelectedObjects(false).Length > 0;
		}*/
	}
}

#endif