﻿using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;
using Newtonsoft.Json.Linq;

namespace MinistryPlatform.Translation.Extensions
{
    public static class JsonUnmappedDataExtensions
    {
        public static T GetUnmappedDataField<T>(this IDictionary<string, JToken> unmappedData, string fieldName)
        {
            return unmappedData.ContainsKey(fieldName) ? unmappedData[fieldName].Value<T>() : default(T);
        }

        public static MpAttributeDto GetAttribute(this IDictionary<string, JToken> unmappedData, string attributePrefix)
        {
            if (!unmappedData.ContainsKey($"{attributePrefix}_Attribute_Name"))
            {
                return null;
            }

            var attr = new MpAttributeDto
            {
                Type = new MpAttributeTypeDto()
            };

            attr.Type.Id = unmappedData.GetUnmappedDataField<int>($"{attributePrefix}_Attribute_Type_ID");
            attr.Type.Name = unmappedData.GetUnmappedDataField<string>($"{attributePrefix}_Attribute_Type_Name");

            attr.Name = unmappedData.GetUnmappedDataField<string>($"{attributePrefix}_Attribute_Name");
            attr.Id = unmappedData.GetUnmappedDataField<int>($"{attributePrefix}_Attribute_ID");
            attr.SortOrder = unmappedData.GetUnmappedDataField<int>($"{attributePrefix}_Attribute_Sort_Order");

            return attr;
        }
    }
}
