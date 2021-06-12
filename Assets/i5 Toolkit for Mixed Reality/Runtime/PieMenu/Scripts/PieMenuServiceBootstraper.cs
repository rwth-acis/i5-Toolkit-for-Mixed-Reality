using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;


namespace i5.Toolkit.MixedReality.PieMenu
{
    public class PieMenuServiceBootstraper : BaseServiceBootstrapper
    {
        public PieMenuSetup toolSetup;
        override protected void RegisterServices()
        {
            CommandStackService commandStackService = new CommandStackService();
            ToolSetupService toolSetupService = new ToolSetupService(toolSetup);

            ServiceManager.RegisterService(commandStackService);
            ServiceManager.RegisterService(toolSetupService);
        }

        protected override void UnRegisterServices()
        {
            ServiceManager.RemoveService<CommandStackService>();
            ServiceManager.RemoveService<ToolSetupService>();
        }
    } 
}
