﻿using Module.Core;
using System;

namespace ESubtitleModule
{
    public class ModuleService : IModuleService
    {
        public ModuleModel GetModule()
        {
            Type moduleAType = typeof(ESubtitleModuleModule);
            ModuleModel module = new ModuleModel
            {
                DisplayName = "ESubtitle",
                ModuleName = moduleAType.Name,
                IsNew = false,
                DefaultView = typeof(Views.ESubtitle)
            };
            return module;
        }
    }
}
