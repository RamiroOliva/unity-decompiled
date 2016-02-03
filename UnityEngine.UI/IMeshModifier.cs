using System;

namespace UnityEngine.UI
{
	public interface IMeshModifier
	{
		/// <summary>
		///   <para>Call used to modify mesh.</para>
		/// </summary>
		/// <param name="mesh"></param>
		[Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts) instead", false)]
		void ModifyMesh(Mesh mesh);

		void ModifyMesh(VertexHelper verts);
	}
}
