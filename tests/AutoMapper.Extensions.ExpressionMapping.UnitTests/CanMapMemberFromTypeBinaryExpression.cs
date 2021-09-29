using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapMemberFromTypeBinaryExpression
    {
        [Fact]
        public void Can_map_using_type_binary_expression_to_test_the_parameter_expression()
        {
            //arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Shape, ShapeDto>()
                    .ForMember(dto => dto.ShapeType, o => o.MapFrom(s => s is Triangle ? ShapeType.Triangle : s is Circle ? ShapeType.Circle : ShapeType.Unknown))
                    .ForMember(dto => dto.DynamicProperties, o => o.Ignore());
            });

            var mapper = config.CreateMapper();
            Expression<Func<ShapeDto, bool>> where = x => x.ShapeType == ShapeType.Circle;

            //act
            Expression<Func<Shape, bool>> whereMapped = mapper.MapExpression<Expression<Func<Shape, bool>>>(where);
            var list = new List<Shape> { new Circle(), new Triangle() }.AsQueryable().Where(whereMapped).ToList();

            //assert
            Assert.Single(list);
            Assert.Equal
            (
                "x => (Convert(IIF((x Is Triangle), Triangle, IIF((x Is Circle), Circle, Unknown)), Int32) == 2)",
                whereMapped.ToString()
            );
        }

        [Fact]
        public void Can_map_using_type_binary_expression_to_test_a_member_expression()
        {
            //arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<ShapeHolder, ShapeHolderDto>();
                cfg.CreateMap<Shape, ShapeDto>()
                    .ForMember(dto => dto.ShapeType, o => o.MapFrom(s => s is Triangle ? ShapeType.Triangle : s is Circle ? ShapeType.Circle : ShapeType.Unknown))
                    .ForMember(dto => dto.DynamicProperties, o => o.Ignore());
            });

            var mapper = config.CreateMapper();
            Expression<Func<ShapeHolderDto, bool>> where = x => x.Shape.ShapeType == ShapeType.Circle;

            //act
            Expression<Func<ShapeHolder, bool>> whereMapped = mapper.MapExpression<Expression<Func<ShapeHolder, bool>>>(where);
            var list = new List<ShapeHolder> 
            { 
                new ShapeHolder { Shape = new Circle() },
                new ShapeHolder { Shape = new Triangle()  }
            }
            .AsQueryable()
            .Where(whereMapped).ToList();

            //assert
            Assert.Single(list);
            Assert.Equal
            (
                "x => (Convert(IIF((x.Shape Is Triangle), Triangle, IIF((x.Shape Is Circle), Circle, Unknown)), Int32) == 2)",
                whereMapped.ToString()
            );
        }

        [Fact]
        public void Can_map_using_instance_method_call_to_test_the_parameter_expression()
        {
            //arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Shape, ShapeDto>()
                    .ForMember(dto => dto.ShapeType, o => o.MapFrom(s => s.GetType() == typeof(Triangle) ? ShapeType.Triangle : s.GetType() == typeof(Circle) ? ShapeType.Circle : ShapeType.Unknown))
                    .ForMember(dto => dto.DynamicProperties, o => o.Ignore());
            });

            var mapper = config.CreateMapper();
            Expression<Func<ShapeDto, bool>> where = x => x.ShapeType == ShapeType.Circle;

            //act
            Expression<Func<Shape, bool>> whereMapped = mapper.MapExpression<Expression<Func<Shape, bool>>>(where);
            var list = new List<Shape> { new Circle(), new Triangle() }.AsQueryable().Where(whereMapped).ToList();

            //assert
            Assert.Single(list);
        }

        [Fact]
        public void Can_map_using_static_method_call_to_test_the_parameter_expression()
        {
            //arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Shape, ShapeDto>()
                    .ForMember(dto => dto.HasMany, o => o.MapFrom(s => s.HasMessages()));
            });

            var mapper = config.CreateMapper();
            Expression<Func<ShapeDto, bool>> where = x => x.HasMany;

            //act
            Expression<Func<Shape, bool>> whereMapped = mapper.MapExpression<Expression<Func<Shape, bool>>>(where);
            var list = new List<Shape> { new Circle() { Messages = new List<string> { "" } }, new Triangle() { Messages = new List<string>() } }.AsQueryable().Where(whereMapped).ToList();

            //assert
            Assert.Single(list);
            Assert.Equal
            (
                "x => x.HasMessages()",
                whereMapped.ToString()
            );
        }

        [Fact]
        public void Can_map_using_static_generic_method_call_to_test_the_parameter_expression()
        {
            //arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.CreateMap<Shape, ShapeDto>()
                    .ForMember(dto => dto.IsCircle, o => o.MapFrom(s => s.IsCircle<Shape>()));
            });

            var mapper = config.CreateMapper();
            Expression<Func<ShapeDto, bool>> where = x => x.IsCircle;

            //act
            Expression<Func<Shape, bool>> whereMapped = mapper.MapExpression<Expression<Func<Shape, bool>>>(where);
            var list = new List<Shape> { new Circle(), new Triangle()}.AsQueryable().Where(whereMapped).ToList();

            //assert
            Assert.Single(list);
            Assert.Equal
            (
                "x => x.IsCircle()",
                whereMapped.ToString()
            );
        }

        public class ShapeHolder
        {
            public Shape Shape { get; set; }
        }

        public class Shape
        {
            public string Name { get; set; }
            public List<string> Messages { get; set; }
        }

        public class Triangle : Shape
        {
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
        }

        public class Circle : Shape
        {
            public double R { get; set; }
        }

        public enum ShapeType
        {
            Unknown,
            Triangle,
            Circle,
        }

        public class ShapeDto
        {
            public string Name { get; set; }
            public IDictionary<string, object> DynamicProperties { get; set; }
            public ShapeType ShapeType { get; set; }
            public bool HasMany { get; set; }
            public bool IsCircle { get; set; }
        }

        public class ShapeHolderDto
        {
            public ShapeDto Shape { get; set; }
        }
    }

    public static class ShapeExtentions
    {
        public static bool HasMessages(this CanMapMemberFromTypeBinaryExpression.Shape shape)
        {
            return shape.Messages.Any();
        }

        public static bool IsCircle<T>(this T shape)
        {
            return shape.GetType() == typeof(CanMapMemberFromTypeBinaryExpression.Circle);
        }
    }
}
