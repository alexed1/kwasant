using System;
using System.Linq;
using AutoMapper;
using Data.Entities;
using KwasantWeb.ViewModels;

namespace KwasantWeb.App_Start
{
    public class AutoMapperBootStrapper
    {

        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<EventDO, EventViewModel>()
                .ForMember(ev => ev.Attendees, opts => opts.ResolveUsing(ev => String.Join(",", ev.Attendees.Select(eea => eea.EmailAddress.Address).Distinct())))
                .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address));
            
            Mapper.CreateMap<EventViewModel, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.Status, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());

            Mapper.CreateMap<EventDO, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());
        }
    }
}