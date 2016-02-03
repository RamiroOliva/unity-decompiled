using System;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
	/// <summary>
	///   <para>A standard toggle that has an on / off state.</para>
	/// </summary>
	[AddComponentMenu("UI/Toggle", 31), RequireComponent(typeof(RectTransform))]
	public class Toggle : Selectable, IEventSystemHandler, IPointerClickHandler, ISubmitHandler, ICanvasElement
	{
		/// <summary>
		///   <para>Display settings for when a toggle is activated or deactivated.</para>
		/// </summary>
		public enum ToggleTransition
		{
			/// <summary>
			///   <para>Show / hide the toggle instantly.</para>
			/// </summary>
			None,
			/// <summary>
			///   <para>Fade the toggle in / out.</para>
			/// </summary>
			Fade
		}

		/// <summary>
		///   <para>UnityEvent callback for when a toggle is toggled.</para>
		/// </summary>
		[Serializable]
		public class ToggleEvent : UnityEvent<bool>
		{
		}

		/// <summary>
		///   <para>Transition mode for the toggle.</para>
		/// </summary>
		public Toggle.ToggleTransition toggleTransition = Toggle.ToggleTransition.Fade;

		/// <summary>
		///   <para>Graphic affected by the toggle.</para>
		/// </summary>
		public Graphic graphic;

		[SerializeField]
		private ToggleGroup m_Group;

		/// <summary>
		///   <para>Callback executed when the value of the toggle is changed.</para>
		/// </summary>
		public Toggle.ToggleEvent onValueChanged = new Toggle.ToggleEvent();

		[FormerlySerializedAs("m_IsActive"), SerializeField, Tooltip("Is the toggle currently on or off?")]
		private bool m_IsOn;

		/// <summary>
		///   <para>Group the toggle belongs to.</para>
		/// </summary>
		public ToggleGroup group
		{
			get
			{
				return this.m_Group;
			}
			set
			{
				this.m_Group = value;
				if (Application.isPlaying)
				{
					this.SetToggleGroup(this.m_Group, true);
					this.PlayEffect(true);
				}
			}
		}

		/// <summary>
		///   <para>Is the toggle on.</para>
		/// </summary>
		public bool isOn
		{
			get
			{
				return this.m_IsOn;
			}
			set
			{
				this.Set(value);
			}
		}

		protected Toggle()
		{
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			this.Set(this.m_IsOn, false);
			this.PlayEffect(this.toggleTransition == Toggle.ToggleTransition.None);
			PrefabType prefabType = PrefabUtility.GetPrefabType(this);
			if (prefabType != PrefabType.Prefab && !Application.isPlaying)
			{
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			}
		}

		/// <summary>
		///   <para>Handling for when the canvas is rebuilt.</para>
		/// </summary>
		/// <param name="executing"></param>
		public virtual void Rebuild(CanvasUpdate executing)
		{
			if (executing == CanvasUpdate.Prelayout)
			{
				this.onValueChanged.Invoke(this.m_IsOn);
			}
		}

		/// <summary>
		///   <para>See ICanvasElement.LayoutComplete.</para>
		/// </summary>
		public virtual void LayoutComplete()
		{
		}

		/// <summary>
		///   <para>See ICanvasElement.GraphicUpdateComplete.</para>
		/// </summary>
		public virtual void GraphicUpdateComplete()
		{
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.SetToggleGroup(this.m_Group, false);
			this.PlayEffect(true);
		}

		/// <summary>
		///   <para>See MonoBehaviour.OnDisable.</para>
		/// </summary>
		protected override void OnDisable()
		{
			this.SetToggleGroup(null, false);
			base.OnDisable();
		}

		protected override void OnDidApplyAnimationProperties()
		{
			if (this.graphic != null)
			{
				bool flag = !Mathf.Approximately(this.graphic.canvasRenderer.GetColor().a, 0f);
				if (this.m_IsOn != flag)
				{
					this.m_IsOn = flag;
					this.Set(!flag);
				}
			}
			base.OnDidApplyAnimationProperties();
		}

		private void SetToggleGroup(ToggleGroup newGroup, bool setMemberValue)
		{
			ToggleGroup group = this.m_Group;
			if (this.m_Group != null)
			{
				this.m_Group.UnregisterToggle(this);
			}
			if (setMemberValue)
			{
				this.m_Group = newGroup;
			}
			if (this.m_Group != null && this.IsActive())
			{
				this.m_Group.RegisterToggle(this);
			}
			if (newGroup != null && newGroup != group && this.isOn && this.IsActive())
			{
				this.m_Group.NotifyToggleOn(this);
			}
		}

		private void Set(bool value)
		{
			this.Set(value, true);
		}

		private void Set(bool value, bool sendCallback)
		{
			if (this.m_IsOn == value)
			{
				return;
			}
			this.m_IsOn = value;
			if (this.m_Group != null && this.IsActive() && (this.m_IsOn || (!this.m_Group.AnyTogglesOn() && !this.m_Group.allowSwitchOff)))
			{
				this.m_IsOn = true;
				this.m_Group.NotifyToggleOn(this);
			}
			this.PlayEffect(this.toggleTransition == Toggle.ToggleTransition.None);
			if (sendCallback)
			{
				this.onValueChanged.Invoke(this.m_IsOn);
			}
		}

		private void PlayEffect(bool instant)
		{
			if (this.graphic == null)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				this.graphic.canvasRenderer.SetAlpha((!this.m_IsOn) ? 0f : 1f);
			}
			else
			{
				this.graphic.CrossFadeAlpha((!this.m_IsOn) ? 0f : 1f, (!instant) ? 0.1f : 0f, true);
			}
		}

		protected override void Start()
		{
			this.PlayEffect(true);
		}

		private void InternalToggle()
		{
			if (!this.IsActive() || !this.IsInteractable())
			{
				return;
			}
			this.isOn = !this.isOn;
		}

		/// <summary>
		///   <para>Handling for when the toggle is 'clicked'.</para>
		/// </summary>
		/// <param name="eventData">Current event.</param>
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
			{
				return;
			}
			this.InternalToggle();
		}

		/// <summary>
		///   <para>Handling for when the submit key is pressed.</para>
		/// </summary>
		/// <param name="eventData">Current event.</param>
		public virtual void OnSubmit(BaseEventData eventData)
		{
			this.InternalToggle();
		}

		virtual bool IsDestroyed()
		{
			return base.IsDestroyed();
		}

		virtual Transform get_transform()
		{
			return base.transform;
		}
	}
}
