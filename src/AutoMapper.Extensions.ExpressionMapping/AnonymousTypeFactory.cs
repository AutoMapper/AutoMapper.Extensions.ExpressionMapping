using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper.Extensions.ExpressionMapping
{
    internal static class AnonymousTypeFactory
    {
        private static int classCount;

        public static Type CreateAnonymousType(IEnumerable<MemberInfo> memberDetails)
            => CreateAnonymousType(memberDetails.ToDictionary(key => key.Name, element => element.GetMemberType()));

        public static Type CreateAnonymousType(IDictionary<string, Type> memberDetails)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly.AutoMapper.Extensions.ExpressionMapping");
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
                        SetMethodBuilder = typeBuilder.DefineMethod(string.Concat("set_", memberName), getSetAttr, null, new Type[] { memberType })
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
