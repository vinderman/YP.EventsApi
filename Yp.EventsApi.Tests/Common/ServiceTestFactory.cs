using AutoMapper;
using Microsoft.Extensions.Logging;
using YP.EventApi.Web.Infrastructure;

namespace Yp.EventsApi.Tests.Common;

internal static class ServiceTestFactory
{
    public static IMapper CreateMapper()
    {
        var logger = new LoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), logger);
        return config.CreateMapper();
    }
}
