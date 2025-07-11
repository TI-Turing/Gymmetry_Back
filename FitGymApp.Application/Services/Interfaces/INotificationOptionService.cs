using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.NotificationOption.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface INotificationOptionService
    {
        ApplicationResponse<NotificationOption> CreateNotificationOption(AddNotificationOptionRequest request);
        ApplicationResponse<NotificationOption> GetNotificationOptionById(Guid id);
        ApplicationResponse<IEnumerable<NotificationOption>> GetAllNotificationOptions();
        ApplicationResponse<bool> UpdateNotificationOption(UpdateNotificationOptionRequest request);
        ApplicationResponse<bool> DeleteNotificationOption(Guid id);
        ApplicationResponse<IEnumerable<NotificationOption>> FindNotificationOptionsByFields(Dictionary<string, object> filters);
    }
}
