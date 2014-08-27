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
            Mapper.CreateMap<EventDO, EventVM>()
                .ForMember(ev => ev.Attendees, opts => opts.ResolveUsing(ev => String.Join(",", ev.Attendees.Select(eea => eea.EmailAddress.Address).Distinct())))
                .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address))
                .ForMember(ev => ev.BookingRequestTimezoneOffsetInMinutes, opts => opts.ResolveUsing(evdo => evdo.DateCreated.Offset.TotalMinutes * - 1));

            Mapper.CreateMap<EventVM, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.EventStatus, opts => opts.Ignore());

            Mapper.CreateMap<EventDO, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());

            Mapper.CreateMap<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
                .ForMember(mu => mu.RemoteCalendars, opts => opts.ResolveUsing(tuple => tuple.Item2
                    .Select(p => new RemoteCalendarVM()
                                     {
                                         Provider = p.Name,
                                         AccessGranted = tuple.Item1.IsRemoteCalendarAccessGranted(p.Name)
                                     })));

        }
    }
}