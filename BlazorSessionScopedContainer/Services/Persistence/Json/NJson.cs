using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services.Persistence.Json
{
    internal class NJson
    {
        public static string SerializeInstance(object instance)
        {
            return JsonSerializer.Serialize(instance);
        }
        public static void DeserializeIntoInstance(string json, object instance, Func<PropertyInfo, object> classInstanciator)
        {
            var properties = instance.GetType().GetProperties().ToList();
            var tokenStream = JsonTokenizer.NormalizeTokenStream(JsonTokenizer.TokenizeJson(json)).GetEnumerator();
            tokenStream.MoveNext(); // {

            while (tokenStream.MoveNext())
            {
                var currentToken = tokenStream.Current;

                if (currentToken.TokenType == TokenType.QuoteSign)
                {
                    string propertyName = "";
                    PropertyInfo? propertyInfo = null;

                    tokenStream.MoveNext();
                    propertyName = tokenStream.Current.Value;
                    propertyInfo = properties.Find(p => p.Name.Equals(propertyName));

                    tokenStream.MoveNext(); // "
                    tokenStream.MoveNext(); // :

                    tokenStream.MoveNext(); // 
                    if (tokenStream.Current.TokenType == TokenType.BracketOpen) // its another class
                    {
                        int brackets = 1;
                        StringBuilder subJsonBuilder = new StringBuilder();
                        subJsonBuilder.Append(tokenStream.Current.Value);
                        while (tokenStream.MoveNext() && brackets > 0)
                        {
                            var current = tokenStream.Current;
                            if (current.TokenType == TokenType.BracketOpen)
                                brackets++;
                            else if (current.TokenType == TokenType.BracketClose)
                                brackets--;

                            subJsonBuilder.Append(current.Value);
                        }
                        var subJson = subJsonBuilder.ToString();
                        var propertyInstance = classInstanciator(propertyInfo);
                        if (propertyInstance == null)
                        {
                            propertyInstance = Activator.CreateInstance(propertyInfo.PropertyType);
                            DeserializeIntoInstance(subJson, propertyInstance, classInstanciator);
                        }

                        propertyInfo.SetValue(instance, propertyInstance);

                    }
                    else if (tokenStream.Current.TokenType == TokenType.QuoteSign || tokenStream.Current.TokenType == TokenType.String)
                    {
                        bool isBetweenLiteral = false;
                        if (tokenStream.Current.TokenType == TokenType.QuoteSign)
                        {
                            tokenStream.MoveNext();
                            isBetweenLiteral = true;
                        }

                        SetPropertyValue(propertyInfo, instance, tokenStream.Current.Value);

                        if (isBetweenLiteral)
                            tokenStream.MoveNext();
                    }
                }
            }
        }
        private static void SetPropertyValue(PropertyInfo propertyInfo, object instance, string value)
        {
            switch (propertyInfo)
            {
                case PropertyInfo prop when prop.PropertyType == typeof(int):
                    propertyInfo.SetValue(instance, int.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(double):
                    propertyInfo.SetValue(instance, double.Parse(value.Replace(".", ",")));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(string):
                    propertyInfo.SetValue(instance, value);
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(long):
                    propertyInfo.SetValue(instance, long.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(byte):
                    propertyInfo.SetValue(instance, byte.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(sbyte):
                    propertyInfo.SetValue(instance, sbyte.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(short):
                    propertyInfo.SetValue(instance, short.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(ushort):
                    propertyInfo.SetValue(instance, ushort.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(uint):
                    propertyInfo.SetValue(instance, uint.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(ulong):
                    propertyInfo.SetValue(instance, ulong.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(float):
                    propertyInfo.SetValue(instance, float.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(decimal):
                    propertyInfo.SetValue(instance, decimal.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(char):
                    propertyInfo.SetValue(instance, char.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(decimal):
                    propertyInfo.SetValue(instance, decimal.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(bool):
                    propertyInfo.SetValue(instance, bool.Parse(value));
                    break;

                case PropertyInfo prop when prop.PropertyType == typeof(DateTime):
                    propertyInfo.SetValue(instance, DateTime.Parse(value));
                    break;

            }
        }

    }
}
