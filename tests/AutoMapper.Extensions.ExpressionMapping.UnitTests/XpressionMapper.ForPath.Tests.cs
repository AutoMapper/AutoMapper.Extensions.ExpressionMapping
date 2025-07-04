using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class XpressionMapperForPathTests
    {
        public XpressionMapperForPathTests()
        {
            SetupAutoMapper();
            SetupQueryableCollection();
        }

        #region Tests
        [Fact]
        public void Works_for_inherited_properties()
        {
            //Arrange
            Expression<Func<DerivedModel, bool>> selection = s => s.Nested.NestedTitle2 == "nested test";

            //Act
            Expression<Func<DerivedData, bool>> selectionMapped = mapper.Map<Expression<Func<DerivedData, bool>>>(selection);
            List<DerivedData> items = DataObjects.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Works_for_inherited_properties_on_base_types()
        {
            //Arrange
            Expression<Func<RootModel, bool>> selection = s => ((DerivedModel)s).Nested.NestedTitle2 == "nested test";

            //Act
            Expression<Func<RootData, bool>> selectionMapped = mapper.MapExpression<Expression<Func<RootData, bool>>>(selection);
            List<RootData> items = DataObjects.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Works_for_top_level_string_member()
        {
            //Arrange
            Expression<Func<Order, bool>> selection = s => s.CustomerHolder.Customer.Name == "Jerry Springer";

            //Act
            Expression<Func<OrderDto, bool>> selectionMapped = mapper.Map<Expression<Func<OrderDto, bool>>>(selection);
            List<OrderDto> items = Orders.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Works_for_top_level_value_type()
        {
            //Arrange
            Expression<Func<Order, bool>> selection = s => s.CustomerHolder.Customer.Age == 32;

            //Act
            Expression<Func<OrderDto, bool>> selectionMapped = mapper.Map<Expression<Func<OrderDto, bool>>>(selection);
            List<OrderDto> items = Orders.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Maps_top_level_string_member_as_include()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Name;

            //Act
            Expression<Func<OrderDto, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection);
            List<object> orders = Orders.Select(selectionMapped).ToList();

            //Assert
            Assert.True(orders.Count == 2);
        }

        [Fact]
        public void Maps_top_level_value_type_as_include()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Total;

            //Act
            Expression<Func<OrderDto, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection);
            List<object> orders = Orders.Select(selectionMapped).ToList();

            //Assert
            Assert.True(orders.Count == 2);
        }

        [Fact]
        public void Throws_exception_when_mapped_value_type_is_a_child_of_the_parameter()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Age;

            //Assert
            Assert.Throws<InvalidOperationException>(() => mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection));
        }

        [Fact]
        public void Throws_exception_when_mapped_string_is_a_child_of_the_parameter()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Address;

            //Assert
            Assert.Throws<InvalidOperationException>(() => mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection));
        }
        #endregion Tests

        private void SetupQueryableCollection()
        {
            DataObjects = new DerivedData[]
            {
                new DerivedData() { OtherID = 2, Title2 = "nested test", ID = 1, Title = "test", DescendantField = "descendant field" },
                new DerivedData() { OtherID = 3, Title2 = "nested", ID = 4, Title = "title", DescendantField = "some text" }
            }.AsQueryable<DerivedData>();

            Orders = new OrderDto[]
            {
                new OrderDto
                {
                     Customer = new CustomerDto{ Name = "George Costanza", Total = 7 },
                     CustomerAddress = "333 First Ave",
                     CustomerAge = 32
                },
                new OrderDto
                {
                     Customer = new CustomerDto{ Name = "Jerry Springer", Total = 8 },
                     CustomerAddress = "444 First Ave",
                     CustomerAge = 31
                }
            }.AsQueryable<OrderDto>();
        }

        private static IQueryable<OrderDto> Orders { get; set; }
        private static IQueryable<DerivedData> DataObjects { get; set; }

        private void SetupAutoMapper()
        {
            var config = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddMaps(typeof(ForPathCustomerProfile));
            });

            mapper = config.CreateMapper();
        }

        static IMapper mapper;
    }

    public class RootModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public NestedModel Nested { get; set; }
    }

    public class NestedModel
    {
        public int NestedID { get; set; }
        public string NestedTitle { get; set; }
        public string NestedTitle2 { get; set; }
    }

    public class DerivedModel : RootModel
    {
        public string DescendantField { get; set; }
    }

    public class RootData
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int OtherID { get; set; }
        public string Title2 { get; set; }
    }

    public class DerivedData : RootData
    {
        public string DescendantField { get; set; }
    }

    public class Order
    {
        public CustomerHolder CustomerHolder { get; set; }
        public int Value { get; }
    }

    public class CustomerHolder
    {
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal? Total { get; set; }
        public decimal? Age { get; set; }
    }

    public class CustomerDto
    {
        public string Name { get; set; }
        public decimal? Total { get; set; }
    }

    public class OrderDto
    {
        public string CustomerAddress { get; set; }
        public decimal? CustomerAge { get; set; }
        public CustomerDto Customer { get; set; }
    }

    public class ForPathCustomerProfile : Profile
    {
        public ForPathCustomerProfile()
        {
            CreateMap<RootData, RootModel>()
                .Include<DerivedData, DerivedModel>();

            CreateMap<DerivedData, DerivedModel>()
                .ForPath(d => d.Nested.NestedTitle, opt => opt.MapFrom(src => src.Title))
                .ForPath(d => d.Nested.NestedTitle2, opt => opt.MapFrom(src => src.Title2))
                .ReverseMap();

            CreateMap<OrderDto, Order>()
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.Customer.Name))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Customer.Total))
                .ForPath(o => o.CustomerHolder.Customer.Address, o => o.MapFrom(s => s.CustomerAddress))
                .ForPath(o => o.CustomerHolder.Customer.Age, o => o.MapFrom(s => s.CustomerAge));
        }
    }
}
