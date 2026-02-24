using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    public class CanMapIfASourceTypeTargetsMultipleDestinationTypesInTheSameExpression
    {
#pragma warning disable xUnit1004 // Test methods should not be skipped
        [Fact(Skip = "This test is currently skipped due to unsupported scenario.")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void Can_map_if_source_type_targets_multiple_destination_types_in_the_same_expression()
        {
            var mapper = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceType, TargetType>().ReverseMap();
                cfg.CreateMap<SourceChildType, TargetChildType>().ReverseMap();

                // Same source type can map to different target types. This seems unsupported currently.
                cfg.CreateMap<SourceListItemType, TargetListItemType>().ReverseMap();
                cfg.CreateMap<SourceListItemType, TargetChildListItemType>().ReverseMap();

            }).CreateMapper();

            Expression<Func<SourceType, bool>> sourcesWithListItemsExpr = src => src.Id != 0 && src.ItemList.Any() && src.Child.ItemList.Any(); // Sources with non-empty ItemList
            Expression<Func<TargetType, bool>> target1sWithListItemsExpr = mapper.MapExpression<Expression<Func<TargetType, bool>>>(sourcesWithListItemsExpr);
        }

#pragma warning disable xUnit1004 // Test methods should not be skipped
        [Fact(Skip = "This test is currently skipped due to unsupported scenario.")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
        public void Can_map_if_source_type_targets_multiple_destination_types_in_the_same_expression_including_nested_parameters()
        {
            var mapper = ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.CreateMap<SourceType, TargetType>().ReverseMap();
                cfg.CreateMap<SourceChildType, TargetChildType>().ReverseMap();

                // Same source type can map to different target types. This seems unsupported currently.
                cfg.CreateMap<SourceListItemType, TargetListItemType>().ReverseMap();
                cfg.CreateMap<SourceListItemType, TargetChildListItemType>().ReverseMap();

            }).CreateMapper();

            Expression<Func<SourceType, bool>> sourcesWithListItemsExpr = src => src.Id != 0 && src.ItemList.FirstOrDefault(i => i.Id == 1) == null && src.Child.ItemList.FirstOrDefault(i => i.Id == 1) == null; // Sources with non-empty ItemList
            Expression<Func<TargetType, bool>> target1sWithListItemsExpr = mapper.MapExpression<Expression<Func<TargetType, bool>>>(sourcesWithListItemsExpr);
        }

        private class SourceChildType
        {
            public int Id { get; set; }
            public IEnumerable<SourceListItemType> ItemList { get; set; } // Uses same type (SourceListItemType) for its itemlist as SourceType
        }

        private class SourceType
        {
            public int Id { get; set; }
            public SourceChildType Child { set; get; }
            public IEnumerable<SourceListItemType> ItemList { get; set; }
        }

        private class SourceListItemType
        {
            public int Id { get; set; }
        }

        private class TargetChildType
        {
            public virtual int Id { get; set; }
            public virtual ICollection<TargetChildListItemType> ItemList { get; set; } = [];
        }

        private class TargetChildListItemType
        {
            public virtual int Id { get; set; }
        }

        private class TargetType
        {
            public virtual int Id { get; set; }

            public virtual TargetChildType Child { get; set; }

            public virtual ICollection<TargetListItemType> ItemList { get; set; } = [];
        }

        private class TargetListItemType
        {
            public virtual int Id { get; set; }
        }
    }
}
