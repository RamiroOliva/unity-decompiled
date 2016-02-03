using System;

namespace UnityEngine.EventSystems
{
	public interface IPointerClickHandler : IEventSystemHandler
	{
		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		void OnPointerClick(PointerEventData eventData);
	}
}
