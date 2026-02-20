using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AutoMapper.Extensions.ExpressionMapping.UnitTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010079dfef85ed6ba841717e154f13182c0a6029a40794a6ecd2886c7dc38825f6a4c05b0622723a01cd080f9879126708eef58f134accdc99627947425960ac2397162067507e3c627992aa6b92656ad3380999b30b5d5645ba46cc3fcc6a1de5de7afebcf896c65fb4f9547a6c0c6433045fceccb1fa15e960d519d0cd694b29a4")]

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal static class AnonymousTypeFactory
    {
        private static int classCount;

        public static Type CreateAnonymousType(IEnumerable<MemberInfo> memberDetails)
            => CreateAnonymousType(memberDetails.ToDictionary(key => key.Name, element => element.GetMemberType()));

        public static Type CreateAnonymousType(IDictionary<string, Type> memberDetails)
        {
            AssemblyName dynamicAssemblyName = new("TempAssembly.AutoMapper.Extensions.ExpressionMapping");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly.AutoMapper.Extensions.ExpressionMapping");
            TypeBuilder typeBuilder = dynamicModule.DefineType(GetAnonymousTypeName(), TypeAttributes.Public);
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var builders = memberDetails.Select
            (
                info =>
                {
                    Type memberType = info.Value;
                    string memberName = info.Key;
                    return new
                    {
                        FieldBuilder = typeBuilder.DefineField(string.Concat("_", memberName), memberType, FieldAttributes.Private),
                        PropertyBuilder = typeBuilder.DefineProperty(memberName, PropertyAttributes.HasDefault, memberType, null),
                        GetMethodBuilder = typeBuilder.DefineMethod(string.Concat("get_", memberName), getSetAttr, memberType, Type.EmptyTypes),
                        SetMethodBuilder = typeBuilder.DefineMethod(string.Concat("set_", memberName), getSetAttr, null, [memberType])
                    };
                }
            );

            builders.ToList().ForEach(builder =>
            {
                ILGenerator getMethodIL = builder.GetMethodBuilder.GetILGenerator();
                getMethodIL.Emit(OpCodes.Ldarg_0);
                getMethodIL.Emit(OpCodes.Ldfld, builder.FieldBuilder);
                getMethodIL.Emit(OpCodes.Ret);

                ILGenerator setMethodIL = builder.SetMethodBuilder.GetILGenerator();
                setMethodIL.Emit(OpCodes.Ldarg_0);
                setMethodIL.Emit(OpCodes.Ldarg_1);
                setMethodIL.Emit(OpCodes.Stfld, builder.FieldBuilder);
                setMethodIL.Emit(OpCodes.Ret);

                builder.PropertyBuilder.SetGetMethod(builder.GetMethodBuilder);
                builder.PropertyBuilder.SetSetMethod(builder.SetMethodBuilder);
            });

            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static string GetAnonymousTypeName()
            => $"AnonymousType{++classCount}";
    }
}
