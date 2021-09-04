using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class XpressionMapperStructsTests
    {
        [Fact]
        public void Can_map_value_types_constants_with_instance_methods()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            List<Garage> source = new List<Garage> {
                new Garage { Truck = default }
            };

            //Act
            var output1 = source.AsQueryable().GetItems<GarageModel, Garage>(mapper, q => q.Truck.Equals(default(TruckModel)));
            var output2 = source.AsQueryable().Query<GarageModel, Garage, GarageModel, Garage>(mapper, q => q.First());

            //Assert
            output1.First().Truck.ShouldBe(default);
            output2.Truck.ShouldBe(default);
        }

        [Fact]
        public void Can_convert_return_type()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            List<Garage> source = new List<Garage> {
                new Garage { Truck = new Truck { Color = "blue", Year = 2000 } }
            };

            //Act
            var output1 = source.AsQueryable().GetItems<GarageModel, Garage>(mapper, q => q.Truck.Year == 2000).Select(g => g.Truck);
            var output2 = source.AsQueryable().Query<GarageModel, Garage, TruckModel, Truck>(mapper, q => q.Select(g => g.Truck).First());

            //Assert
            output1.First().Year.ShouldBe(2000);
            output2.Year.ShouldBe(2000);
        }

        [Fact]
        public void Replace_operator_when_operands_change()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.AddExpressionMapping();
                cfg.CreateMap<Source, Dest>().ReverseMap();
            });

            var mapper = config.CreateMapper();

            Expression<Func<Source, bool>> expression = x => x == default(Source);

            //Act
            var mapped = mapper.MapExpression<Expression<Func<Dest, bool>>>(expression);

            //Assert
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Can_map_local_variable_in_filter()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Truck truck = new Truck { Color = "Red", Year = 1999 };
            Expression<Func<Garage, bool>> filter = m => m.Truck == truck;

            //Act
            var mappedfilter = mapper.MapExpression<Expression<Func<GarageModel, bool>>>(filter);

            //Assert
            Assert.NotNull(mappedfilter);
        }

        [Fact]
        public void Can_map_child_property_of_local_variable_in_filter()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            Garage garage = new Garage { Truck = new Truck { Color = "Red", Year = 1999 } };
            Expression<Func<Garage, bool>> filter = m => m.Truck == garage.Truck;

            //Act
            var mappedfilter = mapper.MapExpression<Expression<Func<GarageModel, bool>>>(filter);

            //Assert
            Assert.NotNull(mappedfilter);
        }

        [Fact]
        public void Can_map_listeral_child_property_of_local_variable_in_filter()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            GarageModel garage = new GarageModel { Color = "Blue", Truck = new TruckModel { Color = "Red", Year = 1999 } };
            Expression<Func<GarageModel, bool>> filter = m => m.Color == garage.Color;

            //Act
            var mappedfilter = mapper.MapExpression<Expression<Func<Garage, bool>>>(filter);

            //Assert
            Assert.NotNull(mappedfilter);
        }

        [Fact]
        public void Ignore_member_expressions_where_type_is_literal_and_node_type_is_constant()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<GarageModel, Garage>()
                    .ReverseMap()
                        .ForMember(d => d.Color, opt => opt.MapFrom(s => s.Truck.Color));
                c.CreateMap<TruckModel, Truck>()
                    .ReverseMap();
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            GarageModel garage = new GarageModel { Color = "Blue", Truck = new TruckModel { Color = "Red", Year = 1999 } };
            Expression<Func<GarageModel, bool>> filter = m => m.Color == garage.Color;

            //Act
            var mappedFilter = mapper.MapExpression<Expression<Func<Garage, bool>>>(filter);
            var mappedrhs = ((BinaryExpression)mappedFilter.Body).Right;

            //Assert
            Assert.Equal(ExpressionType.MemberAccess, mappedrhs.NodeType);
        }

        [Fact]
        public void Can_map_local_variable_literal_in_filter()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ItemWithDateLiteral, ItemWithDateLiteralDto>()
                .ForMember(dest => dest.CreateDate, opts => opts.MapFrom(x => x.Date));
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            DateTime firstReleaseDate = new DateTime();
            DateTime lastReleaseDate = new DateTime();

            Expression<Func<ItemWithDateLiteralDto, bool>> exp = x => (firstReleaseDate == null || x.CreateDate >= firstReleaseDate) &&
                                      (lastReleaseDate == null || x.CreateDate <= lastReleaseDate);

            //Act
            Expression<Func<ItemWithDateLiteral, bool>> expMapped = mapper.MapExpression<Expression<Func<ItemWithDateLiteral, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }

        [Fact]
        public void Can_map_local_variable_nullable_in_filter()
        {
            // Arrange
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ItemWithDateLiteral, ItemWithDateLiteralDto>()
                .ForMember(dest => dest.CreateDate, opts => opts.MapFrom(x => x.Date));
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            DateTime? firstReleaseDate = new DateTime();
            DateTime? lastReleaseDate = new DateTime();

            Expression<Func<ItemWithDateLiteralDto, bool>> exp = x => (firstReleaseDate == null || x.CreateDate >= firstReleaseDate) &&
                                      (lastReleaseDate == null || x.CreateDate <= lastReleaseDate);

            //Act
            Expression<Func<ItemWithDateLiteral, bool>> expMapped = mapper.MapExpression<Expression<Func<ItemWithDateLiteral, bool>>>(exp);

            //Assert
            Assert.NotNull(expMapped);
        }
    }

    public struct Source
    {
        public Source(int i) { val = i; }
        public int val;

        public static implicit operator int(Source i)
        {
            return i.val;
        }

        public static implicit operator Source(int i)
        {
            return new Source(i);
        }

        public static bool operator <(Source a, Source b)
        {
            return a.val < b.val;
        }

        public static bool operator >(Source a, Source b)
        {
            return a.val > b.val;
        }

        public static bool operator ==(Source a, Source b)
        {
            return a.val == b.val;
        }

        public static bool operator ==(Source a, int b)
        {
            return a.val == b;
        }

        public static bool operator ==(int a, Source b)
        {
            return a == b.val;
        }

        public static bool operator !=(Source a, Source b)
        {
            return a.val != b.val;
        }

        public static bool operator !=(Source a, int b)
        {
            return a.val != b;
        }

        public static bool operator !=(int a, Source b)
        {
            return a != b.val;
        }

        public override int GetHashCode()
        {
            return this.val.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Source)
                return this == (Source)obj;
            if (obj is int)
                return this == (Source)((int)obj);
            return false;
        }
    }

    public struct Dest
    {
        public Dest(int i) { val = i; }
        public int val;

        public static implicit operator int(Dest i)
        {
            return i.val;
        }

        public static implicit operator Dest(int i)
        {
            return new Dest(i);
        }

        public static bool operator <(Dest a, Dest b)
        {
            return a.val < b.val;
        }

        public static bool operator >(Dest a, Dest b)
        {
            return a.val > b.val;
        }

        public static bool operator ==(Dest a, Dest b)
        {
            return a.val == b.val;
        }

        public static bool operator ==(Dest a, int b)
        {
            return a.val == b;
        }

        public static bool operator ==(int a, Dest b)
        {
            return a == b.val;
        }

        public static bool operator !=(Dest a, Dest b)
        {
            return a.val != b.val;
        }

        public static bool operator !=(Dest a, int b)
        {
            return a.val != b;
        }

        public static bool operator !=(int a, Dest b)
        {
            return a != b.val;
        }

        public override int GetHashCode()
        {
            return this.val.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Dest)
                return this == (Dest)obj;
            if (obj is int)
                return this == (Dest)((int)obj);
            return false;
        }
    }

    public struct Garage
    {
        public Truck Truck { get; set; }
    }

    public struct GarageModel
    {
        public string Color { get; set; }
        public TruckModel Truck { get; set; }
    }

    public struct Truck
    {
        public string Color { get; set; }
        public int Year { get; set; }

        public static bool operator ==(Truck m1, Truck m2)
        {
            return m1.Year == m2.Year && m1.Color == m2.Color;
        }
        public static bool operator !=(Truck m1, Truck m2)
        {
            return !(m1 == m2);
        }
        public override bool Equals(object obj)
        {
            if (obj is Truck mb)
            {
                return this == mb;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Year.GetHashCode();
        }
    }

    public struct TruckModel
    {
        public string Color { get; set; }
        public int Year { get; set; }

        public static bool operator ==(TruckModel m1, TruckModel m2)
        {
            return m1.Year == m2.Year && m1.Color == m2.Color;
        }
        public static bool operator !=(TruckModel m1, TruckModel m2)
        {
            return !(m1 == m2);
        }
        public override bool Equals(object obj)
        {
            if (obj is TruckModel mb)
            {
                return this == mb;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Year.GetHashCode();
        }
    }

    public struct ItemWithDateLiteralDto
    {
        public DateTime CreateDate { get; set; }
    }

    public struct ItemWithDateLiteral
    {
        public DateTime Date { get; set; }
    }

    static class Extensions
    {
        internal static ICollection<TModel> GetItems<TModel, TData>(this IQueryable<TData> query, IMapper mapper,
        Expression<Func<TModel, bool>> filter = null,
        Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryFunc = null)
        {
            //Map the expressions
            Expression<Func<TData, bool>> f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter);
            Func<IQueryable<TData>, IQueryable<TData>> queryableFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, IQueryable<TData>>>>(queryFunc)?.Compile();

            if (filter != null)
                query = query.Where(f);

            //Call the store
            ICollection<TData> list = queryableFunc != null ? queryableFunc(query).ToList() : query.ToList();

            //Map and return the data
            return mapper.Map<IEnumerable<TData>, IEnumerable<TModel>>(list).ToList();
        }


        internal static TModelResult Query<TModel, TData, TModelResult, TDataResult>(this IQueryable<TData> query, IMapper mapper,
            Expression<Func<IQueryable<TModel>, TModelResult>> queryFunc)
        {
            //Map the expressions
            Func<IQueryable<TData>, TDataResult> mappedQueryFunc = mapper.MapExpression<Expression<Func<IQueryable<TData>, TDataResult>>>(queryFunc).Compile();

            //execute the query
            return mapper.Map<TDataResult, TModelResult>(mappedQueryFunc(query));
        }
    }
}
