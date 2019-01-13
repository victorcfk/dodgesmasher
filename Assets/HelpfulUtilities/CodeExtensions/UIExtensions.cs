using UnityEngine.UI;
using UnityEngine;

public static class UIExtensions {

    /// <summary>
    /// Set the raw image to have the x part of pivot to be at the extreme end and the min x and max x anchor to be at the same place as the pivot
    /// </summary>
    /// <param name="inParentRectWidth"></param>
    /// <param name="inRawImage"></param>
    /// <param name="inCurrentItemCount"></param>
    /// <param name="inMaxItemCount"></param>
    public static void SetScalingUVAsAMeter(this RawImage inRawImage, float inParentRectWidth, float inCurrentItemCount, float inMaxItemCount)
    {
        float inProportion = inCurrentItemCount / inMaxItemCount;
        float rawImageWidth = inProportion * inParentRectWidth;

        inRawImage.rectTransform.offsetMax = new Vector2(rawImageWidth, 0);

        inRawImage.uvRect = new Rect(0, 0, inProportion * inMaxItemCount, inRawImage.uvRect.height);
    }

    /// <summary>
    /// Set the raw image to have the x part of pivot to be at the extreme end and the min x and max x anchor to be at the same place as the pivot
    /// </summary>
    /// <param name="inParentRectWidth"></param>
    /// <param name="inRawImage"></param>
    /// <param name="inCurrentItemCount"></param>
    /// <param name="inMaxItemCount"></param>
    public static void SetUVRect(this RawImage inRawImage, float inWidth, float inHeight =1, float inOffsetX =0, float inOffsetY= 0)
    {
        if (inRawImage == null)
        {
#if UNITY_EDITOR
            Debug.Log("null raw image");
#endif
            return;
        }

        inRawImage.uvRect = new Rect(inOffsetX, inOffsetY, inWidth, inHeight);
    }

    /// <summary>
    /// Shortcut to enable/disable canvas groups
    /// </summary>
    /// <param name="inCanvasGroup"></param>
    /// <param name="inIsActive"></param>
    public static void SetActive(this CanvasGroup inCanvasGroup, bool inIsActive)
    {
        if (inIsActive)
        {
            inCanvasGroup.alpha = 1;
            inCanvasGroup.interactable = true;
            inCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            inCanvasGroup.alpha = 0;
            inCanvasGroup.interactable = false;
            inCanvasGroup.blocksRaycasts = false;
        }
    }
}
