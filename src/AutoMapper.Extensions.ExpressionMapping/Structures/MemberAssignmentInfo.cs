using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    internal class MemberAssignmentInfo
    {
        public MemberAssignmentInfo(PropertyMap propertyMap, MemberAssignment memberAssignment)
        {
            PropertyMap = propertyMap;
            MemberAssignment = memberAssignment;
        }

        /// <summary>
        /// Used to get the source member to be bound with the mapped binding expression.
        /// </summary>
        public PropertyMap PropertyMap { get; set; }

        /// <summary>
        /// Initial member assignment who's binding expression will be mapped and assigned to the source menber of the new type
        /// </summary>
        public MemberAssignment MemberAssignment { get; set; }
    }
}
