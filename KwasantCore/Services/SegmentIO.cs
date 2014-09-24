using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using Segment;
using Segment.Model;
using StructureMap;

namespace KwasantCore.Services
{
    public class SegmentIO : ISegmentIO
    {
        public void Identify(String userID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Identify(uow.UserRepository.GetByKey(userID));
            }
        }

        private Dictionary<String, object> GetProperties(UserDO userDO)
        {
            var user = new User();

            return new Dictionary<string, object>
            {
                {"First Name", userDO.FirstName},
                {"Last Name", userDO.LastName},
                {"Username", userDO.UserName},
                {"Email", userDO.EmailAddress.Address},
                {"Delegate Account", user.GetMode(userDO) == CommunicationMode.DELEGATE }
            };
        }

        public void Identify(UserDO userDO)
        {
            var props = new Traits();
            foreach (var prop in GetProperties(userDO))
                props.Add(prop.Key, prop.Value);
            
            Analytics.Client.Identify(userDO.Id, props);
        }

        public void Track(UserDO userDO, String eventName, String action, Dictionary<String, object> properties = null)
        {
            if (properties == null)
                properties = new Dictionary<string, object>();
            properties["Action"] = action;

            Track(userDO, eventName, properties);
        }

        public void Track(UserDO userDO, String eventName, Dictionary<String, object> properties = null)
        {
            var props = new Segment.Model.Properties();
            foreach (var prop in GetProperties(userDO))
                props.Add(prop.Key, prop.Value);

            if (properties != null)
            {
                foreach (var prop in properties)
                    props[prop.Key] = prop.Value;
            }

            Analytics.Client.Track(userDO.Id, eventName, props);
        }
    }
}
