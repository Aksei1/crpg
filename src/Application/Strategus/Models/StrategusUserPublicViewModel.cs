﻿using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Strategus.Models
{
    /// <summary>
    /// View of a <see cref="StrategusUser"/> when you are not near. That means information like army size or position
    /// are omitted.
    /// </summary>
    public class StrategusUserPublicViewModel : IMapFrom<StrategusUser>
    {
        public int Id { get; set; }
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ClanPublicViewModel? Clan { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StrategusUser, StrategusUserPublicViewModel>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.UserId))
                .ForMember(u => u.Platform, opt => opt.MapFrom(u => u.User!.Platform))
                .ForMember(u => u.PlatformUserId, opt => opt.MapFrom(u => u.User!.PlatformUserId))
                .ForMember(u => u.Name, opt => opt.MapFrom(u => u.User!.Name))
                .ForMember(u => u.Clan, opt => opt.MapFrom(u => u.User!.ClanMembership!.Clan));
        }
    }
}
