using System;
using System.Reflection;

namespace AutoMapper.Extensions.ExpressionMapping.Structures
{
    internal class DeclaringMemberKey(MemberInfo declaringMemberInfo, string declaringMemberFullName) : IEquatable<DeclaringMemberKey>
    {
        public MemberInfo DeclaringMemberInfo { get; set; } = declaringMemberInfo;
        public string DeclaringMemberFullName { get; set; } = declaringMemberFullName;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj is not DeclaringMemberKey key) return false;

            return Equals(key);
        }

        public bool Equals(DeclaringMemberKey other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.DeclaringMemberInfo.Equals(other.DeclaringMemberInfo)
                && this.DeclaringMemberFullName == other.DeclaringMemberFullName;
        }

        public override int GetHashCode() => this.DeclaringMemberInfo.GetHashCode();

        public override string ToString() => this.DeclaringMemberFullName;
    }
}
