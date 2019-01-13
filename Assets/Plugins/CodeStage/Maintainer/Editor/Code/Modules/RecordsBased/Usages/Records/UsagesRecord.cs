#if UNITY_EDITOR

using System;
using System.Text;

namespace CodeStage.Maintainer.Usages
{
	[Serializable]
	public abstract class UsagesRecord : RecordBase
	{
		// ----------------------------------------------------------------------------
		// base constructor
		// ----------------------------------------------------------------------------

		protected UsagesRecord(RecordLocation location)
		{
			this.location = location;
		}

		// ----------------------------------------------------------------------------
		// header generation
		// ----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder header)
		{
			header.Append("Reference");
		}
	}
}
#endif