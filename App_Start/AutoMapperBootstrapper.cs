using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Constants;
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
                .ForMember(ev => ev.CreatedByAddress, opts => opts.ResolveUsing(evdo => evdo.CreatedBy.EmailAddress.Address))
                .ForMember(ev => ev.BookingRequestTimezoneOffsetInMinutes, opts => opts.ResolveUsing(evdo => evdo.DateCreated.Offset.TotalMinutes * - 1));

            Mapper.CreateMap<EventViewModel, EventDO>()
                .ForMember(eventDO => eventDO.Attendees, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.ActivityStatus, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedBy, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.CreatedByID, opts => opts.Ignore())
                .ForMember(eventDO => eventDO.EventStatusID, opts => opts.Ignore());

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
                                                                           QuestionStatusID = QuestionStatus.Unanswered,
                                                                           Text = cr.Question
                                                                       }
                                                               }))
                .ForMember(cr => cr.Recipients, opts => opts.Ignore());

            Mapper.CreateMap<ClarificationRequestDO, ClarificationResponseViewModel>()
                .ForMember(cr => cr.QuestionId, opts => opts.ResolveUsing(cr => cr.Questions.First(q => q.QuestionStatusID == QuestionStatus.Unanswered).Id))
                .ForMember(cr => cr.Question, opts => opts.ResolveUsing(cr => cr.Questions.First(q => q.QuestionStatusID == QuestionStatus.Unanswered).Text))
                .ForMember(cr => cr.Response, opts => opts.ResolveUsing(cr => cr.Questions.First(q => q.QuestionStatusID == QuestionStatus.Unanswered).Response))
                .ForMember(cr => cr.Subject, opts => opts.ResolveUsing(cr => cr.BookingRequest.Subject))
                .ForMember(cr => cr.Body, opts => opts.ResolveUsing(cr => cr.BookingRequest.HTMLText));
            Mapper.CreateMap<ClarificationResponseViewModel, ClarificationRequestDO>()
                .ForMember(cr => cr.Questions,
                           opts => opts.ResolveUsing(cr => new List<QuestionDO>()
                                                               {
                                                                   new QuestionDO()
                                                                       {
                                                                           ClarificationRequestId = cr.Id,
                                                                           Id = cr.QuestionId,
                                                                           Text = cr.Question,
                                                                           Response = cr.Response,
                                                                           QuestionStatusID = QuestionStatus.Answered
                                                                       }
                                                               }));

            Mapper.CreateMap<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserViewModel>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
                .ForMember(mu => mu.RemoteCalendars, opts => opts.ResolveUsing(tuple => tuple.Item2
                    .Select(p => new RemoteCalendarViewModel()
                                     {
                                         Provider = p.Name,
                                         AccessGranted = tuple.Item1.IsRemoteCalendarAccessGranted(p.Name)
                                     })));
        }
    }
}