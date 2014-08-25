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

            Mapper.CreateMap<ClarificationRequestDO, ClarificationRequestVM>()
               
                .ForMember(cr => cr.Recipients, opts => opts.ResolveUsing(ev => String.Join(",", ev.Recipients.Select(eea => eea.EmailAddress.Address).Distinct())));
            Mapper.CreateMap<ClarificationRequestVM, ClarificationRequestDO>()
              
                .ForMember(cr => cr.Recipients, opts => opts.Ignore());

               

               

            Mapper.CreateMap<Tuple<UserDO, IEnumerable<RemoteCalendarProviderDO>>, ManageUserVM>()
                .ForMember(mu => mu.HasLocalPassword, opts => opts.ResolveUsing(tuple => !string.IsNullOrEmpty(tuple.Item1.PasswordHash)))
                .ForMember(mu => mu.RemoteCalendars, opts => opts.ResolveUsing(tuple => tuple.Item2
                    .Select(p => new RemoteCalendarVM()
                                     {
                                         Provider = p.Name,
                                         AccessGranted = tuple.Item1.IsRemoteCalendarAccessGranted(p.Name)
                                     })));

            Mapper.CreateMap<NegotiationDO, EditNegotiationVM>()
                .ForMember(vm => vm.State, opts => opts.ResolveUsing(n => n.NegotiationState))
                .ForMember(vm => vm.Attendees, opts => opts.ResolveUsing(n => n.Attendees.Select(a => a.EmailAddress.Address).ToList()));

            Mapper.CreateMap<EditNegotiationVM, NegotiationDO>()
                .ForMember(n => n.Attendees, opts => opts.Ignore())
                .ForMember(n => n.BookingRequest, opts => opts.Ignore())
                .ForMember(n => n.Calendars, opts => opts.Ignore())
                .ForMember(n => n.Questions, opts => opts
                    .ResolveUsing<ListValueResolver<QuestionVM, QuestionDO>>()
                    .ConstructedBy(() => new ListValueResolver<QuestionVM, QuestionDO>((qvm, q) => qvm.Id == q.Id))
                    .FromMember(vm => vm.Questions))
                .ForMember(n => n.NegotiationState, opts => opts.ResolveUsing(vm => vm.State))
                .ForMember(n => n.NegotiationStateTemplate, opts => opts.UseValue(null));

            Mapper.CreateMap<QuestionDO, QuestionVM>()
                .ForMember(vm => vm.Status, opts => opts.ResolveUsing(q => q.QuestionStatus));

            Mapper.CreateMap<QuestionVM, QuestionDO>()
                .ForMember(q => q.Negotiation, opts => opts.Ignore())
                .ForMember(q => q.Response, opts => opts.Ignore())
                .ForMember(n => n.Answers, opts => opts
                    .ResolveUsing<ListValueResolver<AnswerVM, AnswerDO>>()
                    .ConstructedBy(() => new ListValueResolver<AnswerVM, AnswerDO>((avm, a) => avm.Id == a.Id))
                    .FromMember(vm => vm.Answers))
                .ForMember(q => q.QuestionStatus, opts => opts.ResolveUsing(vm => vm.Status))
                .ForMember(q => q.QuestionStatusTemplate, opts => opts.UseValue(null));

            Mapper.CreateMap<AnswerDO, AnswerVM>()
                .ForMember(vm => vm.AnswerState, opts => opts.ResolveUsing(q => q.AnswerStatus));

            Mapper.CreateMap<AnswerVM, AnswerDO>()
                .ForMember(a => a.AnswerStatus, opts => opts.ResolveUsing(vm => vm.AnswerState))
                .ForMember(n => n.AnswerStatusTemplate, opts => opts.UseValue(null));
        }
    }
}