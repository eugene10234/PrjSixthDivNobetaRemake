using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonEffects:MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("isHover", true);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("isHover", false);
        }
    }
}
