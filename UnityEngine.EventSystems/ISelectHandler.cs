using System;

namespace UnityEngine.EventSystems
{
	public interface ISelectHandler : IEventSystemHandler
	{
		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		void OnSelect(BaseEventData eventData);
	}
}
