﻿using Cike.Core.Modularity;

namespace Cike.Auth;

public class CikeAuthModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        return base.ConfigureServicesAsync(context);
    }

    public override Task InitializeAsync(ApplicationInitializationContext context)
    {
        return base.InitializeAsync(context);
    }
}
