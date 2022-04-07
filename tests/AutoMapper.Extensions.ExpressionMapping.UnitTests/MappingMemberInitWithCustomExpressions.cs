using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class MappingMemberInitWithCustomExpressions
    {
        [Fact]
        public void Map_member_init_using_custom_expressions()
        {
            //Arrange
            var config = GetConfiguration();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerDto, PlayerDto>> selection = s => new PlayerDto
            {
                NameDto = s.NameDto,
                StatsADto = new StatsDto
                {
                    SpeedValueDto = s.StatsADto.SpeedValueDto,
                    PowerDto = s.StatsADto.PowerDto,
                    RatingDto = s.StatsADto.RatingDto,
                    StatsBuilderDto = new BuilderDto
                    {
                        IdDto = s.StatsADto.StatsBuilderDto.IdDto,
                        CityDto = s.StatsADto.StatsBuilderDto.CityDto
                    }
                }
            };

            //Act
            Expression<Func<Player, Player>> selectionMapped = mapper.MapExpression<Expression<Func<Player, Player>>>(selection);
            List<Player> result = Players.Select(selectionMapped).ToList();

            //Assert
            Assert.Equal("Jack", result[0].Name);
            Assert.Equal(1, result[0].StatsA.Power);
            Assert.Equal(2, result[0].StatsA.SpeedValue);
            Assert.Equal(5, result[0].StatsA.Rating);
            Assert.Equal(1, result[0].StatsA.StatsBuilder.Id);
            Assert.Equal("Atlanta", result[0].StatsA.StatsBuilder.City);
        }

        MapperConfiguration GetConfiguration()
            => new
            (
                cfg =>
                {
                    cfg.AddExpressionMapping();

                    cfg.CreateMap<Player, PlayerDto>()
                        .ForMember(dest => dest.NameDto, opt => opt.MapFrom(src => src.Name))
                        .ForMember(dest => dest.StatsADto, opt => opt.MapFrom(src => src.StatsA));

                    cfg.CreateMap<Stats, StatsDto>()
                        .ForMember(dest => dest.SpeedValueDto, opt => opt.MapFrom(src => src.SpeedValue))
                        .ForMember(dest => dest.PowerDto, opt => opt.MapFrom(src => src.Power))
                        .ForMember(dest => dest.RatingDto, opt => opt.MapFrom(src => src.Rating))
                        .ForMember(dest => dest.StatsBuilderDto, opt => opt.MapFrom(src => src.StatsBuilder));

                    cfg.CreateMap<Builder, BuilderDto>()
                        .ForMember(dest => dest.IdDto, opt => opt.MapFrom(src => src.Id))
                        .ForMember(dest => dest.CityDto, opt => opt.MapFrom(src => src.City));
                }
            );

        readonly IQueryable<Player> Players = new List<Player>
        {
            new Player
            {
                Name = "Jack",
                StatsA = new Stats
                {
                    Power = 1,
                    SpeedValue = 2,
                    Rating = 5,
                    StatsBuilder = new Builder
                    {
                        Id = 1,
                        City = "Atlanta"
                    }
                }
            },
            new Player
            {
                Name = "Jane",
                StatsA = new Stats
                {
                    Power = 1,
                    SpeedValue = 3,
                    Rating = 6,
                    StatsBuilder = new Builder
                    {
                        Id = 2,
                        City = "Charlotte"
                    }
                }
            }
        }.AsQueryable();

        public class Player
        {
            public string Name { get; set; }
            public Stats StatsA { get; set; }
        }

        public class Stats
        {
            public int SpeedValue { get; set; }
            public int Power { get; set; }
            public int Rating { get; set; }
            public Builder StatsBuilder { get; set; }
        }

        public class Builder
        {
            public int Id { get; set; }
            public string City { get; set; }
        }

        public class PlayerDto
        {
            public string NameDto { get; set; }
            public StatsDto StatsADto { get; set; }
        }

        public class StatsDto
        {
            public int SpeedValueDto { get; set; }
            public int PowerDto { get; set; }
            public int RatingDto { get; set; }
            public BuilderDto StatsBuilderDto { get; set; }
        }

        public class BuilderDto
        {
            public int IdDto { get; set; }
            public string CityDto { get; set; }
        }
    }
}
