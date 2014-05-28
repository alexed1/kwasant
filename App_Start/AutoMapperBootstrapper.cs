using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using KwasantWeb.ViewModels;

namespace KwasantWeb.App_Start
{
    public class AutoMapperBootStrapper
    {

        public static void ConfigureAutoMapper()
        {


            AutoMapper.Mapper.CreateMap<EventViewModel, EventDO>()
                .ForMember(evt => evt.Attendees, opt => opt.Ignore());

        }
    }
}