using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterEquipSlots.EquipSlots;

// Dragging the panel's window (not a slot or the character) moves the panel; its place is saved when it's let go
internal sealed class PanelDrag : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private bool m_moving;

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_moving = eventData.button == PointerEventData.InputButton.Left;
		if (m_moving)
		{
			SlotPanel.BeginMove();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (m_moving)
		{
			SlotPanel.Move(eventData.delta);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (m_moving)
		{
			m_moving = false;
			SlotPanel.EndMove();
		}
	}

	private void OnDisable()
	{
		if (m_moving)
		{
			m_moving = false;
			SlotPanel.EndMove();
		}
	}
}
