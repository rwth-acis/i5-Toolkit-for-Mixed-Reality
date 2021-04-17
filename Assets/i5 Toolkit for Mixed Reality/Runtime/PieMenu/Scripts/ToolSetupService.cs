using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ToolSetupService : IService
    {
        public ToolSetup toolSetup;

        public ToolSetupService(ToolSetup toolSetup)
        {
            this.toolSetup = toolSetup;
        }

        void IService.Initialize(IServiceManager owner)
        {
        }

        void IService.Cleanup()
        {
        }
    } 
}
