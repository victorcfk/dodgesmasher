using System.Text;

#if UNITY_EDITOR

namespace CodeStage.Maintainer.Usages
{
	public class ObjectUsagesRecord : UsagesRecord
	{
		public string path;
		public string assetDatabasePath;

		public ObjectUsagesRecord(RecordLocation location) : base(location)
		{

		}

		protected override void ConstructCompactLine(StringBuilder text)
		{
			text.Append("Reference");
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("Path").AppendLine(path);
		}
	}
}

#endif