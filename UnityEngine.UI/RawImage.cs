using System;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
	/// <summary>
	///   <para>Displays a Texture2D for the UI System.</para>
	/// </summary>
	[AddComponentMenu("UI/Raw Image", 12)]
	public class RawImage : MaskableGraphic
	{
		[FormerlySerializedAs("m_Tex"), SerializeField]
		private Texture m_Texture;

		[SerializeField]
		private Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

		/// <summary>
		///   <para>The RawImage's texture. (ReadOnly).</para>
		/// </summary>
		public override Texture mainTexture
		{
			get
			{
				if (!(this.m_Texture == null))
				{
					return this.m_Texture;
				}
				if (this.material != null && this.material.mainTexture != null)
				{
					return this.material.mainTexture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		/// <summary>
		///   <para>The RawImage's texture.</para>
		/// </summary>
		public Texture texture
		{
			get
			{
				return this.m_Texture;
			}
			set
			{
				if (this.m_Texture == value)
				{
					return;
				}
				this.m_Texture = value;
				this.SetVerticesDirty();
				this.SetMaterialDirty();
			}
		}

		/// <summary>
		///   <para>The RawImage texture coordinates.</para>
		/// </summary>
		public Rect uvRect
		{
			get
			{
				return this.m_UVRect;
			}
			set
			{
				if (this.m_UVRect == value)
				{
					return;
				}
				this.m_UVRect = value;
				this.SetVerticesDirty();
			}
		}

		protected RawImage()
		{
			base.useLegacyMeshGeneration = false;
		}

		/// <summary>
		///   <para>Adjusts the raw image size to make it pixel-perfect.</para>
		/// </summary>
		public override void SetNativeSize()
		{
			Texture mainTexture = this.mainTexture;
			if (mainTexture != null)
			{
				int num = Mathf.RoundToInt((float)mainTexture.width * this.uvRect.width);
				int num2 = Mathf.RoundToInt((float)mainTexture.height * this.uvRect.height);
				base.rectTransform.anchorMax = base.rectTransform.anchorMin;
				base.rectTransform.sizeDelta = new Vector2((float)num, (float)num2);
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			Texture mainTexture = this.mainTexture;
			vh.Clear();
			if (mainTexture != null)
			{
				Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
				Vector4 vector = new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
				Color color = base.color;
				vh.AddVert(new Vector3(vector.x, vector.y), color, new Vector2(this.m_UVRect.xMin, this.m_UVRect.yMin));
				vh.AddVert(new Vector3(vector.x, vector.w), color, new Vector2(this.m_UVRect.xMin, this.m_UVRect.yMax));
				vh.AddVert(new Vector3(vector.z, vector.w), color, new Vector2(this.m_UVRect.xMax, this.m_UVRect.yMax));
				vh.AddVert(new Vector3(vector.z, vector.y), color, new Vector2(this.m_UVRect.xMax, this.m_UVRect.yMin));
				vh.AddTriangle(0, 1, 2);
				vh.AddTriangle(2, 3, 0);
			}
		}
	}
}
