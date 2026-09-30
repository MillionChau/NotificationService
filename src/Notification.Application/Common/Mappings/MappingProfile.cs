using AutoMapper;
using Notification.Application.DTOs;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NotificationEntity, NotificationDto>();
    }
}
