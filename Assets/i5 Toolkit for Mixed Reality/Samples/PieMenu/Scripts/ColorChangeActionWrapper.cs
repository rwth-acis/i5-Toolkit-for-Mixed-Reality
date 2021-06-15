using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;

public class ColorChangeActionWrapper : MonoBehaviour
{
    public Color[] colors;
    int currentColorIndex = 0;

    public void IncreaseColor(BaseInputEventData data)
    {
        int newColorIndex = mod((currentColorIndex + 1), colors.Length);
        ChangeColor(data, colors[newColorIndex]);
        currentColorIndex = newColorIndex;
    }

    public void DecreaseColor(BaseInputEventData data)
    {
        int newColorIndex = mod((currentColorIndex - 1), colors.Length);
        ChangeColor(data, colors[newColorIndex]);
        currentColorIndex = newColorIndex;
    }

    void ChangeColor(BaseInputEventData data, Color color)
    {
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);

        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        target = objectTransformer.TransformObject(target, ActionHelperFunctions.GetCurrentToolName(data.InputSource));

        ColorChangeAction colorChangeAction = new ColorChangeAction(target, color);
        ServiceManager.GetService<CommandStackService>().AddAndPerformAction(colorChangeAction);
    }

    int mod(int a, int n)
    {
        int result = a % n;
        if ((result < 0 && n > 0) || (result > 0 && n < 0))
        {
            result += n;
        }
        return result;
    }
}
