using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Newtonsoft.Json;
using Utilities;

namespace Data.Repositories
{
    public class FactRepository : GenericRepository<FactDO>, IFactRepository
    {
        internal FactRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public IEnumerable<String> GetJournalingForBookingRequest(int bookingRequestId)
        {
            return GetJournalingForBookingRequest(UnitOfWork.BookingRequestRepository.GetByKey(bookingRequestId));
        }

        public IEnumerable<String> GetJournalingForBookingRequest(BookingRequestDO bookingRequestDO)
        {
            var bookingRequestFacts = GetJournalingFor("BookingRequest", bookingRequestDO.Id);

            var negotiationRequestIDs = bookingRequestDO.Negotiations.Select(n => n.Id);
// ReSharper disable PossibleMultipleEnumeration
            var negotiationRequestFacts = GetJournalingFor("NegotiationRequest", negotiationRequestIDs, bookingRequestDO.Id);
            var negotiationRequestQuestionIDs = bookingRequestDO.Negotiations.SelectMany(n => n.Questions.Select(q => q.Id));
            var negotiationRequestQuestionFacts = GetJournalingFor("Question", negotiationRequestQuestionIDs, negotiationRequestIDs);

            var negotiationRequestAnswerIDs = bookingRequestDO.Negotiations.SelectMany(n => n.Questions.SelectMany(q => q.Answers.Select(a => a.Id)));
            var negotiationRequestAnswerFacts = GetJournalingFor("Answer", negotiationRequestAnswerIDs, negotiationRequestQuestionIDs);
// ReSharper restore PossibleMultipleEnumeration

            var fullJournal =
                bookingRequestFacts.Union(negotiationRequestFacts)
                    .Union(negotiationRequestQuestionFacts)
                    .Union(negotiationRequestAnswerFacts).ToList();

            return Verbalise(fullJournal);
        }

        private static IEnumerable<string> Verbalise(IEnumerable<FactDO> fullJournal)
        {
            foreach (var journal in fullJournal.OrderByDescending(j => j.CreateDate))
            {
                var userName = journal.CreatedBy == null ? "Unknown" : journal.CreatedBy.UserName;
                var deserialized = JsonConvert.DeserializeObject(journal.Status);
                var status = deserialized == null ? String.Empty : deserialized.ToString();
                var timeString = journal.CreateDate.TimeAgo();
                switch (journal.Activity)
                {
                    case "Update":
                        yield return
                            String.Format("[{0}] - {1}. New value: '{2}' by {3} {4}", journal.PrimaryCategory, journal.Name, status, userName, timeString);
                        break;
                    case "Create":
                        yield return
                            String.Format("[{0}] [{1}] - Created by {2} {3}", journal.PrimaryCategory, status, userName, timeString);
                        break;
                    case "Delete":
                        yield return
                            String.Format("[{0}] [{1}] - Deleted by {2} {3}", journal.PrimaryCategory, status, userName, timeString);
                        break;
                }
            }
        }

        private IQueryable<FactDO> GetJournalingFor(String contextName, int objectID)
        {
            return GetJournalingFor(contextName, new[] {objectID});
        }

        private IQueryable<FactDO> GetJournalingFor(String contextName, IEnumerable<int> objectIDs, int? parentID = null)
        {
            return GetJournalingFor(contextName, objectIDs, parentID.HasValue ? new[] {parentID.Value} : new int[] {});
        }

        private IQueryable<FactDO> GetJournalingFor(String contextName, IEnumerable<int> objectIDs, IEnumerable<int> parentIDs)
        {
            // ReSharper disable PossibleMultipleEnumeration
            // ReSharper disable ImplicitlyCapturedClosure
            var query =
                //Updates and creates
                GetQuery().Where(
                f => f.PrimaryCategory == contextName &&

                     f.SecondaryCategory == "Journaling" &&
                     objectIDs.Contains(f.ObjectId)
                );

            if (parentIDs.Any())
            {
                //Deletes
                query = query.Union(
                    GetQuery().Where(
                        f => f.PrimaryCategory == contextName &&
                             f.SecondaryCategory == "Journaling" &&
                             f.Activity == "Delete" &&
                             parentIDs.Contains(f.TaskId)
                        )
                    );
            }
            return query;
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore ImplicitlyCapturedClosure

        }
    }


    public interface IFactRepository : IGenericRepository<FactDO>
    {

    }
}
