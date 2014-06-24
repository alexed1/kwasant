using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Entities.Enumerations;
using KwasantWeb.ViewModels;

namespace KwasantWeb.App_Start
{
    public class AutoMapperBootStrapper
    {

        public static void ConfigureAutoMapper()
        {
            Mapper.CreateMap<EventDO, EventViewModel>()
                .ForMember(ev => ev.Attendees, opts => opts.ResolveUsing(ev => String.Join(",", ev.Attendees.Select(eea => eea.EmailAddress.Address).Distinct())))
                .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address))
                .ForMember(ev => ev.BookingRequestTimezoneOffsetInMinutes, opts => opts.ResolveUsing(evdo => evdo.BookingRequest.DateCreated.Offset.TotalMinutes * - 1));

            Mapper.CreateMap<EventViewModel, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());

            Mapper.CreateMap<EventDO, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore());

            Mapper.CreateMap<ClarificationRequestDO, ClarificationRequestViewModel>()
                .ForMember(cr => cr.QuestionsCount, opts => opts.ResolveUsing(cr => cr.Questions.Count))
                .ForMember(cr => cr.Recipients, opts => opts.ResolveUsing(ev => String.Join(",", ev.Recipients.Select(eea => eea.EmailAddress.Address).Distinct())));
            Mapper.CreateMap<ClarificationRequestViewModel, ClarificationRequestDO>()
                .ForMember(cr => cr.Questions,
                           opts => opts.ResolveUsing(cr => new List<QuestionDO>()
                                                               {
                                                                   new QuestionDO()
                                                                       {
                                                                           ClarificationRequestId = cr.Id,
                                                                           Status = QuestionStatus.Unanswered,
                                                                           Text = cr.Question
                                                                       }
                                                               }))
                .ForMember(cr => cr.Recipients, opts => opts.Ignore());
        }
    }
}