using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace apiplate.Helpers
{
    public class ManualMapper 
    {
        public  TDest ManualMap<TSource, TDest>(TSource source, TDest dest,IList<Func<TSource, bool>> conditions = null,string[] propsToExclude = null )
        {
            propsToExclude = propsToExclude ?? Array.Empty<string>();
            var sourceProps = source.GetType().GetProperties();
            var destProps = dest.GetType().GetProperties();

            foreach (var prop in destProps)
            {
                var propName = prop.Name;
                var propValue = prop.GetValue(dest);
                foreach (var sourceProp in sourceProps)
                {
                    var sourcePropName = sourceProp.Name;
                    var sourcePropValue = sourceProp.GetValue(source);
                    if (propName == sourcePropName 
                    && prop.PropertyType == sourceProp.PropertyType 
                    && sourcePropValue != default && propsToExclude.Contains(propName) == false)
                    {
                        if(conditions != null)
                        foreach (var condition in conditions)
                        {
                            var result = condition.Invoke(source);
                            if(result == false)
                            continue;
                        }
                        if (prop.PropertyType.IsPrimitive
                           || prop.PropertyType == typeof(Decimal)
                           || prop.PropertyType == typeof(String) || prop.PropertyType == typeof(DateTime))
                        {

                            prop.SetValue(dest,sourcePropValue);
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(source.GetType()))
                        {
                            var listPropValue = propValue as IEnumerable;
                            var listSourcePropValue = sourcePropValue as IEnumerable;

                            foreach (var value in listPropValue)
                            {
                                foreach (var sourceValue in listSourcePropValue)
                                {
                                    var manualMapMethodInfo = this.GetType().GetMethod("ManualMap");
                                    manualMapMethodInfo.MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                                    .Invoke(this, new[] { sourceValue, value,null,null});
                                }

                            }
                        }
                        else
                        {
                            var manualMapMethodInfo = this.GetType().GetMethod("ManualMap");
                            manualMapMethodInfo.MakeGenericMethod(sourceProp.PropertyType, prop.PropertyType)
                            .Invoke(this, new[] { sourcePropValue, propValue,null,null});
                        }
                    }
                }
            }
            return dest;
        }
        
    }
}