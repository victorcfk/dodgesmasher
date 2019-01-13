using UnityEngine;

#if UNITY_EDITOR

namespace CodeStage.Maintainer.Usages
{
	public struct ProjectAsset
	{
		public string assetPath;
		public Object unityObject;

		public ProjectAsset(string assetPath, Object unityObject)
		{
			this.assetPath = assetPath;
			this.unityObject = unityObject;
		}

		public override string ToString()
		{
			return "Object: " + unityObject + "\nPath: " + assetPath;
		}
	}
}

#endif