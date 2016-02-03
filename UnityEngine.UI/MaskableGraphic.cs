using System;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
	/// <summary>
	///   <para>A Graphic that is capable of being mased out.</para>
	/// </summary>
	public abstract class MaskableGraphic : Graphic, IMaskable, IClippable, IMaterialModifier
	{
		[Serializable]
		public class CullStateChangedEvent : UnityEvent<bool>
		{
		}

		[NonSerialized]
		protected bool m_ShouldRecalculateStencil = true;

		[NonSerialized]
		protected Material m_MaskMaterial;

		[NonSerialized]
		private RectMask2D m_ParentMask;

		[NonSerialized]
		private bool m_Maskable = true;

		[Obsolete("Not used anymore.", true)]
		[NonSerialized]
		protected bool m_IncludeForMasking;

		[SerializeField]
		private MaskableGraphic.CullStateChangedEvent m_OnCullStateChanged = new MaskableGraphic.CullStateChangedEvent();

		[Obsolete("Not used anymore", true)]
		[NonSerialized]
		protected bool m_ShouldRecalculate = true;

		[NonSerialized]
		protected int m_StencilValue;

		private readonly Vector3[] m_Corners = new Vector3[4];

		/// <summary>
		///   <para>Callback issued when culling changes.</para>
		/// </summary>
		public MaskableGraphic.CullStateChangedEvent onCullStateChanged
		{
			get
			{
				return this.m_OnCullStateChanged;
			}
			set
			{
				this.m_OnCullStateChanged = value;
			}
		}

		/// <summary>
		///   <para>Does this graphic allow masking.</para>
		/// </summary>
		public bool maskable
		{
			get
			{
				return this.m_Maskable;
			}
			set
			{
				if (value == this.m_Maskable)
				{
					return;
				}
				this.m_Maskable = value;
				this.m_ShouldRecalculateStencil = true;
				this.SetMaterialDirty();
			}
		}

		private Rect canvasRect
		{
			get
			{
				base.rectTransform.GetWorldCorners(this.m_Corners);
				if (base.canvas)
				{
					for (int i = 0; i < 4; i++)
					{
						this.m_Corners[i] = base.canvas.transform.InverseTransformPoint(this.m_Corners[i]);
					}
				}
				return new Rect(this.m_Corners[0].x, this.m_Corners[0].y, this.m_Corners[2].x - this.m_Corners[0].x, this.m_Corners[2].y - this.m_Corners[0].y);
			}
		}

		/// <summary>
		///   <para>See IMaterialModifier.GetModifiedMaterial.</para>
		/// </summary>
		/// <param name="baseMaterial"></param>
		public virtual Material GetModifiedMaterial(Material baseMaterial)
		{
			Material material = baseMaterial;
			if (this.m_ShouldRecalculateStencil)
			{
				Transform stopAfter = MaskUtilities.FindRootSortOverrideCanvas(base.transform);
				this.m_StencilValue = ((!this.maskable) ? 0 : MaskUtilities.GetStencilDepth(base.transform, stopAfter));
				this.m_ShouldRecalculateStencil = false;
			}
			if (this.m_StencilValue > 0 && base.GetComponent<Mask>() == null)
			{
				Material maskMaterial = StencilMaterial.Add(material, (1 << this.m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << this.m_StencilValue) - 1, 0);
				StencilMaterial.Remove(this.m_MaskMaterial);
				this.m_MaskMaterial = maskMaterial;
				material = this.m_MaskMaterial;
			}
			return material;
		}

		/// <summary>
		///   <para>See IClippable.Cull.</para>
		/// </summary>
		/// <param name="clipRect"></param>
		/// <param name="validRect"></param>
		public virtual void Cull(Rect clipRect, bool validRect)
		{
			if (!base.canvasRenderer.hasMoved)
			{
				return;
			}
			bool flag = !validRect || !clipRect.Overlaps(this.canvasRect, true);
			bool flag2 = base.canvasRenderer.cull != flag;
			base.canvasRenderer.cull = flag;
			if (flag2)
			{
				this.m_OnCullStateChanged.Invoke(flag);
				this.SetVerticesDirty();
			}
		}

		/// <summary>
		///   <para>See IClippable.SetClipRect.</para>
		/// </summary>
		/// <param name="clipRect"></param>
		/// <param name="validRect"></param>
		public virtual void SetClipRect(Rect clipRect, bool validRect)
		{
			if (validRect)
			{
				base.canvasRenderer.EnableRectClipping(clipRect);
			}
			else
			{
				base.canvasRenderer.DisableRectClipping();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.m_ShouldRecalculateStencil = true;
			this.UpdateClipParent();
			this.SetMaterialDirty();
		}

		/// <summary>
		///   <para>See MonoBehaviour.OnDisable.</para>
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			this.m_ShouldRecalculateStencil = true;
			this.SetMaterialDirty();
			this.UpdateClipParent();
			StencilMaterial.Remove(this.m_MaskMaterial);
			this.m_MaskMaterial = null;
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			this.m_ShouldRecalculateStencil = true;
			this.UpdateClipParent();
			this.SetMaterialDirty();
		}

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			this.m_ShouldRecalculateStencil = true;
			this.UpdateClipParent();
			this.SetMaterialDirty();
		}

		/// <summary>
		///   <para>See: IMaskable.</para>
		/// </summary>
		[Obsolete("Not used anymore.", true)]
		public virtual void ParentMaskStateChanged()
		{
		}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged();
			this.m_ShouldRecalculateStencil = true;
			this.UpdateClipParent();
			this.SetMaterialDirty();
		}

		private void UpdateClipParent()
		{
			RectMask2D rectMask2D = (!this.maskable || !this.IsActive()) ? null : MaskUtilities.GetRectMaskForClippable(this);
			if (rectMask2D != this.m_ParentMask && this.m_ParentMask != null)
			{
				this.m_ParentMask.RemoveClippable(this);
			}
			if (rectMask2D != null)
			{
				rectMask2D.AddClippable(this);
			}
			this.m_ParentMask = rectMask2D;
		}

		/// <summary>
		///   <para>See: IClippable.RecalculateClipping.</para>
		/// </summary>
		public virtual void RecalculateClipping()
		{
			this.UpdateClipParent();
		}

		/// <summary>
		///   <para>See: IMaskable.RecalculateMasking.</para>
		/// </summary>
		public virtual void RecalculateMasking()
		{
			this.m_ShouldRecalculateStencil = true;
			this.SetMaterialDirty();
		}

		virtual RectTransform get_rectTransform()
		{
			return base.rectTransform;
		}
	}
}
