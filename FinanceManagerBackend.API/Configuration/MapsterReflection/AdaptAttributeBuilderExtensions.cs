using System.Linq.Expressions;
using Mapster;

namespace FinanceManagerBackend.API.Configuration.MapsterReflection;

public static class AdaptAttributeBuilderExtensions
{
    /// <summary>
    /// Вызывает builder.ForType&lt;TType&gt;(cfg => cfg.Map(x => x.propertyName, Nullable&lt;PropertyType&gt;)) через рефлексию.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type">TType.</param>
    /// <param name="propertyName">Наименование поля.</param>
    public static void MakePropertyNullable(this AdaptAttributeBuilder builder, Type type, string propertyName)
    {
        var propertyType = type.GetProperty(propertyName)!.PropertyType;
        var newPropertyType = typeof(Nullable<>).MakeGenericType(propertyType);

        var forType = typeof(AdaptAttributeBuilder).GetMethod("ForType")!.MakeGenericMethod(type);

        var cfgT = typeof(PropertySettingBuilder<>).MakeGenericType(type);
        var actionT = typeof(Action<>).MakeGenericType(cfgT);

        // cfg =>
        var cfgParam = Expression.Parameter(cfgT, "cfg");

        // x => x.propertyName
        var xParam = Expression.Parameter(type, "x");
        var idProp = Expression.Property(xParam, propertyName);
        var memberLambda = Expression.Lambda(idProp, xParam);

        // cfg.Map(x => x.propertyName, typeof(Nullable<propertyType>))
        var mapMethod = cfgT.GetMethods()
            .First(x => x.Name == "Map" && x.GetParameters().Length == 3)
            .MakeGenericMethod(propertyType);
        var call = Expression
            .Call(cfgParam, mapMethod, memberLambda, Expression.Constant(newPropertyType, typeof(Type)),
                Expression.Constant(null, typeof(string)));

        var lambda = Expression.Lambda(actionT, call, cfgParam).Compile();

        forType.Invoke(builder, new[] { lambda });
    }

    /// <summary>
    /// Вызывает builder.ForType&lt;TType&gt;(cfg => cfg.Ignore(x => x.propertyName)) через рефлексию.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type">TType.</param>
    /// <param name="propertyName">Наименование поля.</param>
    public static void IgnoreProperty(this AdaptAttributeBuilder builder, Type type, string propertyName)
    {
        var propertyType = type.GetProperty(propertyName)!.PropertyType;

        var forType = typeof(AdaptAttributeBuilder).GetMethod("ForType")!.MakeGenericMethod(type);

        var cfgT = typeof(PropertySettingBuilder<>).MakeGenericType(type);
        var actionT = typeof(Action<>).MakeGenericType(cfgT);

        // cfg =>
        var cfgParam = Expression.Parameter(cfgT, "cfg");

        // x => x.propertyName
        var xParam = Expression.Parameter(type, "x");
        var idProp = Expression.Property(xParam, propertyName);
        var memberLambda = Expression.Lambda(idProp, xParam);

        // cfg.Ignore(x => x.propertyName)
        var ignoreMethod = cfgT.GetMethod("Ignore")!.MakeGenericMethod(propertyType);
        var call = Expression
            .Call(cfgParam, ignoreMethod, memberLambda);

        var lambda = Expression.Lambda(actionT, call, cfgParam).Compile();

        forType.Invoke(builder, new[] { lambda });
    }


}