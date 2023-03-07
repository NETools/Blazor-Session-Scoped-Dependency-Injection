using BlazorSessionScopedContainer.Contracts;
using BlazorSessionScopedContainer.Core;
using BlazorSessionScopedContainer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Misc.Json
{
    internal class NJson
    {
        public object Deserialize(Guid id, string json, object instance)
        {
            var properties = instance.GetType().GetProperties().ToList();

            JsonTokenizer tokenizer = new JsonTokenizer();
            
            var tokenStream = tokenizer.NormalizeTokenStream(tokenizer.TokenizeJson(json)).GetEnumerator();
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
                        object propertyInstance = null;
                        if (propertyInfo.PropertyType.GetInterfaces().Contains(typeof(ISessionScoped)))
                        {
                            if (NSessionHandler.Default().ServiceInstances.ContainsKey(id))
                                propertyInstance = NSessionHandler.Default().ServiceInstances[id].Find(p => p.AreServicesEqual(propertyInfo.PropertyType)).GetServiceInstance();
                        }
                        else
                        {
                            propertyInstance = Deserialize(id, subJson, Activator.CreateInstance(propertyInfo.PropertyType));
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

                        switch (propertyInfo)
                        {
                            case PropertyInfo prop when prop.PropertyType == typeof(int):
                                propertyInfo.SetValue(instance, int.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(double):
                                propertyInfo.SetValue(instance, double.Parse(tokenStream.Current.Value.Replace(".", ",")));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(string):
                                propertyInfo.SetValue(instance, tokenStream.Current.Value);
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(long):
                                propertyInfo.SetValue(instance, long.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(byte):
                                propertyInfo.SetValue(instance, byte.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(sbyte):
                                propertyInfo.SetValue(instance, sbyte.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(short):
                                propertyInfo.SetValue(instance, short.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(ushort):
                                propertyInfo.SetValue(instance, ushort.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(uint):
                                propertyInfo.SetValue(instance, uint.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(ulong):
                                propertyInfo.SetValue(instance, ulong.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(float):
                                propertyInfo.SetValue(instance, float.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(decimal):
                                propertyInfo.SetValue(instance, decimal.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(char):
                                propertyInfo.SetValue(instance, char.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(decimal):
                                propertyInfo.SetValue(instance, decimal.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(bool):
                                propertyInfo.SetValue(instance, bool.Parse(tokenStream.Current.Value));
                                break;

                            case PropertyInfo prop when prop.PropertyType == typeof(DateTime):
                                propertyInfo.SetValue(instance, DateTime.Parse(tokenStream.Current.Value));
                                break;

                        }

                        if (isBetweenLiteral)
                            tokenStream.MoveNext();
                    }
                }
            }

            return instance;
        }

    }
}
