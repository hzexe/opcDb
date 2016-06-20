using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Syncer.EventAction
{
    public static class DynamicType
    {
        /*
        public static Type DynamicCreateType()
        {
            //动态创建程序集
            AssemblyName DemoName = new AssemblyName("DbDynamicAssembly");
            AssemblyBuilder dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName, AssemblyBuilderAccess.RunAndSave);
            //动态创建模块
            ModuleBuilder mb = dynamicAssembly.DefineDynamicModule(DemoName.Name, DemoName.Name + ".dll");
            //动态创建类MyClass
            TypeBuilder tb = mb.DefineType("datavalue", TypeAttributes.Public);
            //动态创建字段
            FieldBuilder fb = tb.DefineField("myField", typeof(System.String), FieldAttributes.Private);
            //动态创建构造函数
            Type[] clorType = new Type[] { typeof(System.String) };
            ConstructorBuilder cb1 = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, clorType);
            //生成指令
            ILGenerator ilg = cb1.GetILGenerator();//生成 Microsoft 中间语言 (MSIL) 指令
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Stfld, fb);
            ilg.Emit(OpCodes.Ret);
            //动态创建属性
            PropertyBuilder pb = tb.DefineProperty("MyProperty", PropertyAttributes.HasDefault, typeof(string), null);
            //动态创建方法
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName;
            MethodBuilder myMethod = tb.DefineMethod("get_Field", getSetAttr, typeof(string), Type.EmptyTypes);
            //生成指令
            ILGenerator numberGetIL = myMethod.GetILGenerator();
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fb);
            numberGetIL.Emit(OpCodes.Ret);
            //使用动态类创建类型
            Type classType = tb.CreateType();
            //保存动态创建的程序集 (程序集将保存在程序目录下调试时就在Debug下)
            dynamicAssembly.Save(DemoName.Name + ".dll");
            //创建类
            return classType;
        }
        */


        public static TypeBuilder CreateTypeBuilder(string assemblyName, string moduleName, string typeName)
        {
            TypeBuilder typeBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.Run).DefineDynamicModule(moduleName).DefineType(typeName,
            TypeAttributes.Public);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder;
        }


        public static void CreateAutoImplementedProperty(TypeBuilder builder, string propertyName, Type propertyType, Type[][] parameterTypeRequiredCustomModifiers=null,
    Type[][] parameterTypeOptionalCustomModifiers=null)
        {
            const string PrivateFieldPrefix = "m_";
            const string GetterPrefix = "get_";
            const string SetterPrefix = "set_";

            // Generate the field.
            FieldBuilder fieldBuilder = builder.DefineField(
            string.Concat(
            PrivateFieldPrefix, propertyName),
            propertyType,
            FieldAttributes.Private);

            // Generate the property
            PropertyBuilder propertyBuilder = builder.DefineProperty(
            propertyName,
            System.Reflection.PropertyAttributes.HasDefault,
            propertyType,
            null,
            null,
            null,
            parameterTypeRequiredCustomModifiers,
            parameterTypeOptionalCustomModifiers
            );

            // Property getter and setter attributes.
            MethodAttributes propertyMethodAttributes = MethodAttributes.Public
            | MethodAttributes.SpecialName
            | MethodAttributes.HideBySig;

            // Define the getter method.
            MethodBuilder getterMethod = builder.DefineMethod(
            string.Concat(
            GetterPrefix, propertyName),
            propertyMethodAttributes,
            propertyType,
            Type.EmptyTypes);

            // Emit the IL code.
            // ldarg.0
            // ldfld,_field
            // ret
            ILGenerator getterILCode = getterMethod.GetILGenerator();
            getterILCode.Emit(OpCodes.Ldarg_0);
            getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
            getterILCode.Emit(OpCodes.Ret);

            // Define the setter method.
            MethodBuilder setterMethod = builder.DefineMethod(
            string.Concat(SetterPrefix, propertyName),
            propertyMethodAttributes,
            null,
            new Type[] { propertyType });

            // Emit the IL code.
            // ldarg.0
            // ldarg.1
            // stfld,_field
            // ret
            ILGenerator setterILCode = setterMethod.GetILGenerator();
            setterILCode.Emit(OpCodes.Ldarg_0);
            setterILCode.Emit(OpCodes.Ldarg_1);
            setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
            setterILCode.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            propertyBuilder.SetSetMethod(setterMethod);
        }
    }
}
