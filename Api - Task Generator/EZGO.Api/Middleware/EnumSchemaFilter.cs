using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace EZGO.Api.Middleware
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();

                var enumNames = Enum.GetNames(context.Type).ToList();
                var enumValues = new List<int>();

                foreach (var enumValue in Enum.GetValues(context.Type))
                {
                    var convertedIntValue = Convert.ToInt32(enumValue);
                    enumValues.Add(convertedIntValue);
                }

                if (enumNames.Count == enumValues.Count)
                {
                    for (int i = 0; i < enumNames.Count; i++)
                    {
                        model.Enum.Add(new OpenApiObject()
                        {
                            {"key", new OpenApiString(enumNames[i])},
                            {"value", new OpenApiInteger(enumValues[i])},
                            {"description", new OpenApiString(context.Type.GetField(enumNames[i])?.GetCustomAttribute<DescriptionAttribute>(false)?.Description ?? "")}
                        });
                    }
                }

                model.Type = "object";
                model.Format = string.Empty;
            }
        }
    }
}
