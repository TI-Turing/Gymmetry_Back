using AutoMapper;
using Gymmetry.Domain.DTO.AccessMethodType.Request;
using Gymmetry.Domain.DTO.Bill.Request;
using Gymmetry.Domain.DTO.Branch.Request;
using Gymmetry.Domain.DTO.Brand.Request;
using Gymmetry.Domain.DTO.Daily.Request;
using Gymmetry.Domain.DTO.DailyExercise.Request;
using Gymmetry.Domain.DTO.Exercise.Request;
using Gymmetry.Domain.DTO.FitUser.Request;
using Gymmetry.Domain.DTO.Gym.Request;
using Gymmetry.Domain.DTO.GymPlanSelected.Request;
using Gymmetry.Domain.DTO.Notification.Request;
using Gymmetry.Domain.DTO.Plan.Request;
using Gymmetry.Domain.DTO.RoutineDay.Request;
using Gymmetry.Domain.DTO.RoutineExercise.Request;
using Gymmetry.Domain.DTO.UserBlock;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Domain.DTO.PostShare.Request;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Domain.Models;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<AddRoutineExerciseRequest, RoutineExercise>();
        CreateMap<UpdateRoutineExerciseRequest, RoutineExercise>();
        CreateMap<AddFitUserRequest, FitUser>()
            .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.Goal))
            .ForMember(dest => dest.ExperienceLevel, opt => opt.MapFrom(src => src.ExperienceLevel));
        CreateMap<UpdateFitUserRequest, FitUser>()
            .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.Goal))
            .ForMember(dest => dest.ExperienceLevel, opt => opt.MapFrom(src => src.ExperienceLevel));
        CreateMap<AddExerciseRequest, Exercise>();
        CreateMap<UpdateExerciseRequest, Exercise>();
        CreateMap<AddAccessMethodTypeRequest, AccessMethodType>();
        CreateMap<UpdateAccessMethodTypeRequest, AccessMethodType>();
        CreateMap<AddBillRequest, Bill>();
        CreateMap<UpdateBillRequest, Bill>();
        CreateMap<AddBranchRequest, Branch>();
        CreateMap<UpdateBranchRequest, Branch>();
        CreateMap<AddBrandRequest, Brand>();
        CreateMap<UpdateBrandRequest, Brand>();
        CreateMap<AddNotificationRequest, Notification>();
        CreateMap<UpdateNotificationRequest, Notification>();
        CreateMap<NotificationCreateRequestDto, Notification>();
        CreateMap<AddGymRequest, Gym>();
        CreateMap<UpdateGymRequest, Gym>();
        CreateMap<AddGymPlanSelectedRequest, GymPlanSelected>();
        CreateMap<AddRoutineDayRequest, RoutineDay>();
        CreateMap<AddPlanRequest, Plan>();
        CreateMap<AddDailyRequest, Daily>();
        CreateMap<AddDailyExerciseRequest, DailyExercise>();
        CreateMap<Gymmetry.Domain.DTO.ReportContent.ReportContentCreateRequest, ReportContent>();
        CreateMap<Gymmetry.Domain.DTO.ReportContent.ReportContentUpdateRequest, ReportContent>();
        CreateMap<ReportContent, Gymmetry.Domain.DTO.ReportContent.ReportContentResponse>();

        // UserBlock mappings
        CreateMap<UserBlockCreateRequest, UserBlock>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.BlockerId, opt => opt.Ignore())
            .ForMember(dest => dest.Blocker, opt => opt.Ignore())
            .ForMember(dest => dest.BlockedUser, opt => opt.Ignore());

        CreateMap<UserBlock, UserBlockResponse>()
            .ForMember(dest => dest.BlockerName, opt => opt.MapFrom(src => src.Blocker != null ? src.Blocker.Name : null))
            .ForMember(dest => dest.BlockedUserName, opt => opt.MapFrom(src => src.BlockedUser != null ? src.BlockedUser.Name : null));

        // ContentModeration mappings
        CreateMap<ContentModerationCreateRequest, ContentModeration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ModeratedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Ip, opt => opt.Ignore())
            .ForMember(dest => dest.Moderator, opt => opt.Ignore());

        CreateMap<ContentModerationUpdateRequest, ContentModeration>()
            .ForMember(dest => dest.ContentId, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.Ignore())
            .ForMember(dest => dest.ModerationReason, opt => opt.Ignore())
            .ForMember(dest => dest.ModeratedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AutoModerated, opt => opt.Ignore())
            .ForMember(dest => dest.FilterType, opt => opt.Ignore())
            .ForMember(dest => dest.Confidence, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Ip, opt => opt.Ignore())
            .ForMember(dest => dest.Moderator, opt => opt.Ignore());

        CreateMap<ContentModeration, ContentModerationResponse>()
            .ForMember(dest => dest.ModeratorName, opt => opt.MapFrom(src => src.Moderator != null ? src.Moderator.Name : null));

        // PostShare mappings
        CreateMap<AddPostShareRequest, PostShare>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Ip, opt => opt.Ignore())
            .ForMember(dest => dest.Post, opt => opt.Ignore())
            .ForMember(dest => dest.SharedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.SharedWithUser, opt => opt.Ignore());

        CreateMap<PostShare, PostShareResponse>();
    }
}