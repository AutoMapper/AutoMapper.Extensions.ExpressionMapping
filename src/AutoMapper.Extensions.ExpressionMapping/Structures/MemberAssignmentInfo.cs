using System.Linq.Expressions;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    internal class MemberAssignmentInfo(PropertyMap propertyMap, MemberAssignment memberAssignment)
    {

        /// <summary>
        /// Used to get the source member to be bound with the mapped binding expression.
        /// </summary>
        public PropertyMap PropertyMap { get; set; } = propertyMap;

        /// <summary>
        /// Initial member assignment who's binding expression will be mapped and assigned to the source menber of the new type
        /// </summary>
        public MemberAssignment MemberAssignment { get; set; } = memberAssignment;
    }
}
