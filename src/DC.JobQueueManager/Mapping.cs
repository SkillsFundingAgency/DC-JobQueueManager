//using System;
//using System.Collections.Generic;
//using System.Text;
//using AutoMapper;
//using ESFA.DC.JobQueueManager.Data.Entities;
//using ESFA.DC.Jobs.Model;

//namespace ESFA.DC.JobQueueManager
//{
//    public static class Mapping
//    {
//        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
//        {
//            var config = new MapperConfiguration(cfg =>
//            {
//                cfg.AddProfile<MappingProfile>();
//            });
//            var mapper = config.CreateMapper();
//            return mapper;
//        });

//        public static IMapper Mapper => Lazy.Value;
//    }

//    public class MappingProfile : Profile
//    {
//        public MappingProfile()
//        {
//            CreateMap<JobEntity, IlrJob>();
//        }
//    }
//}
