using System;
using System.Collections.Generic;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    /// <summary>
    /// Defines the type to be initialized and a list of source bindings.
    /// The new bound members will be matched using MemberAssignmentInfos.PropertyMap and 
    /// assigned to the mapped expression (mapped from MemberAssignmentInfos.MemberAssignment.Expression).
    /// </summary>
    internal class MemberBindingGroup(DeclaringMemberKey declaringMemberKey, bool isRootMemberAssignment, Type newType, List<MemberAssignmentInfo> memberAssignmentInfos)
    {

        /// <summary>
        /// DeclaringMemberKey will be null when the member assignment is a member binding of OldType on the initial (root) TypeMap (OldType -> NewType)
        /// </summary>
        public DeclaringMemberKey DeclaringMemberKey { get; set; } = declaringMemberKey;

        /// <summary>
        /// MemberAssignment is true if it is a member binding of OldType on the initial (root) TypeMap (OldType -> NewType)
        /// </summary>
        public bool IsRootMemberAssignment { get; set; } = isRootMemberAssignment;

        /// <summary>
        /// Destination type of the member assignment. If IsRootMemberAssignment == true then this is the destination type of initial (root) TypeMap (OldType -> NewType)
        /// Otherwise it is the PropertyType/FieldType of DeclaringMemberInfo
        /// </summary>
        public Type NewType { get; set; } = newType;

        /// <summary>
        /// List of members to be mapped and bound to the new type
        /// </summary>
        public List<MemberAssignmentInfo> MemberAssignmentInfos { get; set; } = memberAssignmentInfos;
    }
}
