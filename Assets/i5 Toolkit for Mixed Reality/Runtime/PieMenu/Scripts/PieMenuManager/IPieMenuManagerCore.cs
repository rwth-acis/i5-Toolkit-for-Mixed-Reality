using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IPieMenuManagerCore
    {
        IPieMenuManagerShell shell { get; set; }
        void MenuOpen(BaseInputEventData eventData, bool pieMenuInstatiated, ToolSetupService toolSetupService,
            ref IMixedRealityInputSource invokingSource);
        void MenuClose(BaseInputEventData eventData, bool pieMenuInstatiated, ToolSetupService toolSetupService,
            int currentlyHighlighted, ref IMixedRealityInputSource invokingSource);
    }
}
