using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using KwasantWeb.ViewModels;
using ViewModel.Models;

namespace KwasantWeb.App_Start
{
    public class AutoMapperBootStrapper
    {

        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<EventDO, EventViewModel>()
                .ForMember(ev => ev.Attendees, opts => opts.ResolveUsing(ev => String.Join(",", ev.Attendees.Select(eea => eea.EmailAddress.Address).Distinct())))
                .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address))
                .ForMember(ev => ev.BookingRequestTimezoneOffsetInMinutes, opts => opts.ResolveUsing(evdo => evdo.DateCreated.Offset.TotalMinutes * - 1));

            Mapper.CreateMap<EventViewModel, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.EventStatus, opts => opts.Ignore());

            Mapper.CreateMap<EventDO, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());

            Mapper.CreateMap<ClarificationRequestDO, ClarificationRequestViewModel>()
               
                .ForMember(cr => cr.Recipients, opts => opts.ResolveUsing(ev => String.Join(",", ev.Recipients.Select(eea => eea.EmailAddress.Address).Distinct())));
            Mapper.CreateMap<ClarificationRequestViewModel, ClarificationRequestDO>()
              
                .ForMember(cr => cr.Recipients, opts => opts.Ignore());

               

               

            Mapper.CreateMap<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserViewModel>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
                .ForMember(mu => mu.RemoteCalendars, opts => opts.ResolveUsing(tuple => tuple.Item2
                    .Select(p => new RemoteCalendarViewModel()
                                     {
                                         Provider = p.Name,
                                         AccessGranted = tuple.Item1.IsRemoteCalendarAccessGranted(p.Name)
                                     })));

            Mapper.CreateMap<NegotiationDO, NegotiationViewModel>()
                .ForMember(vm => vm.RequestId, opts => opts.ResolveUsing(n => n.BookingRequestID))
                .ForMember(vm => vm.AttendeeList, opts => opts.Ignore());

            Mapper.CreateMap<NegotiationViewModel, NegotiationDO>()
                .ForMember(n => n.BookingRequest, opts => opts.Ignore())
                .ForMember(n => n.BookingRequestID, opts => opts.Ignore())
                .ForMember(n => n.Calendars, opts => opts.Ignore())
                .ForMember(n => n.Id, opts => opts.Ignore())
                .ForMember(n => n.NegotiationStateTemplate, opts => opts.UseValue(null));

            Mapper.CreateMap<QuestionViewModel, QuestionDO>()
                .ForMember(q => q.Id, opts => opts.Ignore())
                .ForMember(q => q.Negotiation, opts => opts.Ignore())
                .ForMember(q => q.Response, opts => opts.Ignore())
                .ForMember(q => q.QuestionStatus, opts => opts.ResolveUsing(vm => vm.Status.Id))
                .ForMember(q => q.QuestionStatusTemplate, opts => opts.ResolveUsing(vm => vm.Status));

            Mapper.CreateMap<AnswerViewModel, AnswerDO>()
                .ForMember(a => a.AnswerStatus, opts => opts.ResolveUsing(vm => vm.AnswerStatusId))
                .ForMember(n => n.AnswerStatusTemplate, opts => opts.UseValue(null));
        }
    }
}