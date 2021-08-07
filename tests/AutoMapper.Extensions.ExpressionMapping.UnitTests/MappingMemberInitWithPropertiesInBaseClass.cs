using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class MappingMemberInitWithPropertiesInBaseClass
    {
        [Fact]
        public void ShouldMapWhenParenIsConfiguredUsingIncludeMembers()
        {
            //Arrange
            var config = GetConfigurationWiithIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            //Act
            List<PlayerModel> models = mapper.ProjectTo<PlayerModel>(Players).ToList();

            //Assert
            Assert.Equal("Jack", models[0].Name);
            Assert.Equal(1, models[0].StatsAPower);
            Assert.Equal(2, models[0].StatsASpeed);
            Assert.Equal(5, models[0].StatsARating);
            Assert.Equal(1, models[0].StatsABuilderId);
            Assert.Equal("Atlanta", models[0].StatsABuilderCity);

            //Assert.Equal(2, models[0].StatsBPower);
            //Assert.Equal(3, models[0].StatsBSpeed);
            //Assert.Equal(7, models[0].StatsBRating);
            //Assert.Equal(3, models[0].StatsBBuilderId);
            //Assert.Equal("Columbia", models[0].StatsBBuilderCity);
        }

        [Fact]
        public void Map_member_init_with_include_members()
        {
            //Arrange
            var config = GetConfigurationWiithIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerModel, PlayerModel>> selection = s => new PlayerModel { Name = s.Name, StatsASpeed = s.StatsASpeed, StatsAPower = s.StatsAPower, StatsARating = s.StatsARating };

            //Act
            Expression<Func<Player, Player>> selectionMapped = mapper.MapExpression<Expression<Func<Player, Player>>>(selection);
            List<Player> result = Players.Select(selectionMapped).ToList();

            //Assert
            Assert.Equal("Jack", result[0].Name);
            Assert.Equal(1, result[0].StatsA.Power);
            Assert.Equal(2, result[0].StatsA.SpeedValue);
            Assert.Equal(5, result[0].StatsA.Rating);
        }

        [Fact]
        public void Map_member_init_with_include_members_and_nested_include_members()
        {
            //Arrange
            var config = GetConfigurationWiithIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerModel, PlayerModel>> selection = s => new PlayerModel
            {
                Name = s.Name,
                StatsASpeed = s.StatsASpeed,
                StatsAPower = s.StatsAPower,
                StatsARating = s.StatsARating,
                StatsABuilderId = s.StatsABuilderId,
                StatsABuilderCity = s.StatsABuilderCity,
                StatsBSpeed = s.StatsBSpeed,
                StatsBPower = s.StatsBPower,
                StatsBRating = s.StatsBRating,
                StatsBBuilderId = s.StatsBBuilderId,
                StatsBBuilderCity = s.StatsBBuilderCity
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

            //Assert.Equal(2, result[0].StatsB.Power);
            //Assert.Equal(3, result[0].StatsB.SpeedValue);
            //Assert.Equal(7, result[0].StatsB.Rating);
            //Assert.Equal(3, result[0].StatsB.StatsBuilder.Id);
            //Assert.Equal("Columbia", result[0].StatsB.StatsBuilder.City);
        }

        [Fact]
        public void Map_member_expression_with_include_members_and_nested_include_members()
        {
            //Arrange
            var config = GetConfigurationWiithIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerModel, string>> selection = p => p.StatsABuilderCity;

            //Act
            Expression<Func<Player, string>> selectionMapped = mapper.MapExpression<Expression<Func<Player, string>>>(selection);
            List<string> result = Players.Select(selectionMapped).ToList();

            //Assert
            Assert.Equal("Atlanta", result[0]);
            Assert.Equal("p => p.StatsA.StatsBuilder.City", selectionMapped.ToString());
        }

        [Fact]
        public void Map_member_init_without_include_members()
        {
            //Arrange
            var config = GetConfigurationWiithoutIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerModel, PlayerModel>> selection = s => new PlayerModel { Name = s.Name, StatsASpeed = s.StatsASpeed, StatsAPower = s.StatsAPower, StatsARating = s.StatsARating };

            //Act
            Expression<Func<Player, Player>> selectionMapped = mapper.MapExpression<Expression<Func<Player, Player>>>(selection);
            List<Player> result = Players.Select(selectionMapped).ToList();

            //Assert
            Assert.Equal("Jack", result[0].Name);
            Assert.Equal(1, result[0].StatsA.Power);
            Assert.Equal(2, result[0].StatsA.SpeedValue);
            Assert.Equal(5, result[0].StatsA.Rating);
        }

        [Fact]
        public void Map_member_init_without_include_members_and_including_nested_members()
        {
            //Arrange
            var config = GetConfigurationWiithoutIncludeMembers();
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Expression<Func<PlayerModel, PlayerModel>> selection = s => new PlayerModel
            {
                Name = s.Name,
                StatsASpeed = s.StatsASpeed,
                StatsAPower = s.StatsAPower,
                StatsARating = s.StatsARating,
                StatsABuilderId = s.StatsABuilderId,
                StatsABuilderCity = s.StatsABuilderCity,
                StatsBSpeed = s.StatsBSpeed,
                StatsBPower = s.StatsBPower,
                StatsBRating = s.StatsBRating,
                StatsBBuilderId = s.StatsBBuilderId,
                StatsBBuilderCity = s.StatsBBuilderCity
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

            Assert.Equal(2, result[0].StatsB.Power);
            Assert.Equal(3, result[0].StatsB.SpeedValue);
            Assert.Equal(7, result[0].StatsB.Rating);
            Assert.Equal(3, result[0].StatsB.StatsBuilder.Id);
            Assert.Equal("Columbia", result[0].StatsB.StatsBuilder.City);
        }

        MapperConfiguration GetConfigurationWiithoutIncludeMembers()
            => new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Player, PlayerModel>()
                    .ForMember(d => d.StatsABuilderId, opt => opt.MapFrom(s => s.StatsA.StatsBuilder.Id))
                    .ForMember(d => d.StatsABuilderCity, opt => opt.MapFrom(s => s.StatsA.StatsBuilder.City))
                    .ForMember(d => d.StatsAPower, opt => opt.MapFrom(s => s.StatsA.Power))
                    .ForMember(d => d.StatsASpeed, opt => opt.MapFrom(s => s.StatsA.SpeedValue))
                    .ForMember(d => d.StatsBBuilderId, opt => opt.MapFrom(s => s.StatsB.StatsBuilder.Id))
                    .ForMember(d => d.StatsBBuilderCity, opt => opt.MapFrom(s => s.StatsB.StatsBuilder.City))
                    .ForMember(d => d.StatsBPower, opt => opt.MapFrom(s => s.StatsB.Power))
                    .ForMember(d => d.StatsBSpeed, opt => opt.MapFrom(s => s.StatsB.SpeedValue));
            });

        MapperConfiguration GetConfigurationWiithIncludeMembers()
            => new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Player, PlayerModel>()
                    .IncludeMembers(p => p.StatsA);

                cfg.CreateMap<Stats, PlayerModel>()
                    .ForMember(d => d.Name, opt => opt.Ignore())
                    .ForMember(d => d.StatsARating, opt => opt.MapFrom(s => s.Rating))
                    .ForMember(d => d.StatsBRating, opt => opt.MapFrom(s => s.Rating))
                    .ForMember(d => d.StatsASpeed, opt => opt.MapFrom(s => s.SpeedValue))
                    .ForMember(d => d.StatsBSpeed, opt => opt.MapFrom(s => s.SpeedValue))
                    .ForMember(d => d.StatsAPower, opt => opt.MapFrom(s => s.Power))
                    .ForMember(d => d.StatsBPower, opt => opt.MapFrom(s => s.Power))
                    .IncludeMembers(p => p.StatsBuilder);

                cfg.CreateMap<Builder, PlayerModel>()
                    .ForMember(d => d.StatsABuilderCity, opt => opt.MapFrom(s => s.City))
                    .ForMember(d => d.StatsABuilderId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(d => d.StatsBBuilderCity, opt => opt.MapFrom(s => s.City))
                    .ForMember(d => d.StatsBBuilderId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(d => d.Name, opt => opt.Ignore())
                    .ForMember(d => d.StatsASpeed, opt => opt.Ignore())
                    .ForMember(d => d.StatsAPower, opt => opt.Ignore())
                    .ForMember(d => d.StatsARating, opt => opt.Ignore())
                    .ForMember(d => d.StatsBSpeed, opt => opt.Ignore())
                    .ForMember(d => d.StatsBPower, opt => opt.Ignore())
                    .ForMember(d => d.StatsBRating, opt => opt.Ignore());
            });

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
                },
                StatsB = new Stats
                {
                    Power = 2,
                    SpeedValue = 3,
                    Rating = 7,
                    StatsBuilder = new Builder
                    {
                        Id = 3,
                        City = "Columbia"
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
                },
                StatsB = new Stats
                {
                    StatsBuilder = new Builder()
                }
            }
        }.AsQueryable();

        public class Player : PlayerBase
        {
        }

        public class PlayerBase
        {
            public string Name { get; set; }
            public Stats StatsA { get; set; }
            public Stats StatsB { get; set; }
        }

        public class Stats : StatsBase
        {
        }

        public class StatsBase
        {
            public int SpeedValue { get; set; }
            public int Power { get; set; }
            public int Rating { get; set; }
            public Builder StatsBuilder { get; set; }
        }

        public class Builder : BuilderBase
        {
        }

        public class BuilderBase
        {
            public int Id { get; set; }
            public string City { get; set; }
        }

        public class PlayerModel : PlayerModelBase
        {
        }

        public class PlayerModelBase
        {
            public string Name { get; set; }
            public int StatsASpeed { get; set; }
            public int StatsAPower { get; set; }
            public int StatsARating { get; set; }
            public int StatsABuilderId { get; set; }
            public string StatsABuilderCity { get; set; }
            public int StatsBSpeed { get; set; }
            public int StatsBPower { get; set; }
            public int StatsBRating { get; set; }
            public int StatsBBuilderId { get; set; }
            public string StatsBBuilderCity { get; set; }
        }
    }
}
