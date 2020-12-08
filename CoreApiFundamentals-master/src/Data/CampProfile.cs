using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        /*we're going to create a map from camp -> CampModel
        the way create map works in automapper is it's going to allow us to create a default map it's going to attempt to match all the properties 
        in the way we want then its going to allow us with syntax to make exceptions to map manually [e.g single field venue]
            */
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName)).ReverseMap();

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap();

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();
        }

    }
}
