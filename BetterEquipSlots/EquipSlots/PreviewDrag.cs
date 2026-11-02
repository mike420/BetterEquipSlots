using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterEquipSlots.EquipSlots;

internal sealed class PreviewDrag : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IScrollHandler
{
	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnScroll(PointerEventData eventData)
	{
		PlayerPreview.Zoom(eventData.scrollDelta.y);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.dragging)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				PlayerPreview.Attack();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				PlayerPreview.Block();
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
		{
			PlayerPreview.Turn(eventData.delta.x);
			PlayerPreview.Pan(eventData.delta.y);
		}
	}
}
