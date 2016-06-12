using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using IRSI.SOSFileUploader.ApiClients;

namespace IRSI.SOSFileUploader.Modules
{
    public class SOSUploadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenClient>().AsSelf();
            builder.RegisterType<SOSApiClient>().AsSelf();
            builder.RegisterType<SOSFileUploader>().AsSelf();
        }
    }
}
