﻿using Detached.DataAnnotations.Plugins.KeyAnnotation;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

namespace Detached.EntityFramework.Plugins.PartialKey
{
    public class PartialKeyEntityConvention : IEntityTypeAddedConvention
    {
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            SortedList<int, PropertyInfo> pKey = new SortedList<int, PropertyInfo>();
            foreach (Property property in entityTypeBuilder.Metadata.GetProperties())
            {
                if (property.PropertyInfo != null)
                {
                    PKeyAttribute pkeyAttribute = property.PropertyInfo.GetCustomAttribute<PKeyAttribute>();
                    if (pkeyAttribute != null)
                    {
                        pKey.Add(pkeyAttribute.Order, property.PropertyInfo);
                    }
                }
            }

            if (pKey.Count > 0)
            {
                entityTypeBuilder.PrimaryKey(pKey.Values.ToArray(), ConfigurationSource.Explicit);
            }

            return entityTypeBuilder;
        }
    }
}
