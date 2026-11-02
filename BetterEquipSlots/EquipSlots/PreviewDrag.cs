using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterEquipSlots.EquipSlots;

internal sealed class PreviewDrag : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public void OnEndDrag(PointerEventData eventData)
	{
		PlayerPreview.ReturnToStart();
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
		}
	}
}
