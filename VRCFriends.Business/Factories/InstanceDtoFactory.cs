using Microsoft.Extensions.DependencyInjection;
using System;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Models;
using VRChat.API.Model;

namespace VRCFriends.Business.Factories
{
    public class InstanceDtoFactory : IInstanceDtoFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public InstanceDtoFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public InstanceDto Create(string worldName, InstanceType? instanceType = null, InstanceRegion? region = null)
        {
            if (_serviceProvider is null)
                throw new NullReferenceException(nameof(_serviceProvider));

            var service = _serviceProvider.GetRequiredService<InstanceDto>();

            service.WorldName = worldName;
            service.InstanceType = instanceType;
            service.Region = region;

            return service;
        }
    }
}
