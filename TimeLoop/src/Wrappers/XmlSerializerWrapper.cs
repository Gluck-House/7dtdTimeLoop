using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TimeLoop.Wrappers
{
    public static class XmlSerializerWrapper
    {
        public static T FromXml<T>(string path)
        {
            StreamReader reader = new StreamReader(path);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T obj = (T)xmlSerializer.Deserialize(reader);
            reader.Close();
            return obj;
        }

        public static void ToXml<T>(string path, T obj)
        {
            TextWriter writer = new StreamWriter(path);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(writer, obj);
            writer.Close();
        }

        public static void FromXmlOverwrite<T>(string path, T obj)
        {
            T objNew = FromXml<T>(path);
            CopyPublicMembers(objNew, obj);
        }

        public static bool HasMissingSerializedMembers<T>(string path, T reference)
        {
            XDocument document = XDocument.Load(path);
            XElement? root = document.Root;
            if (root == null)
                return true;

            return HasMissingSerializedMembers(typeof(T), root, reference);
        }

        private static void CopyPublicMembers<T>(T source, T destination)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (property.GetIndexParameters().Length > 0)
                    continue;

                property.SetValue(destination, property.GetValue(source));
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(destination, field.GetValue(source));
            }
        }

        private static bool HasMissingSerializedMembers(Type type, XElement element, object? reference)
        {
            XNamespace xmlNamespace = element.Name.Namespace;

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
                    continue;

                if (!ShouldSerializeMember(type, property.Name, reference))
                    continue;

                var elementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName
                                  ?? property.GetCustomAttribute<XmlArrayAttribute>()?.ElementName
                                  ?? property.Name;
                if (string.IsNullOrWhiteSpace(elementName))
                    continue;

                var childElement = element.Element(xmlNamespace + elementName);
                if (childElement == null)
                    return true;

                if (property.GetCustomAttribute<XmlArrayAttribute>() != null || !ShouldRecurseInto(property.PropertyType))
                    continue;

                var childReference = reference == null ? null : property.GetValue(reference);
                if (childReference != null && HasMissingSerializedMembers(property.PropertyType, childElement, childReference))
                    return true;
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var elementName = field.GetCustomAttribute<XmlElementAttribute>()?.ElementName
                                  ?? field.GetCustomAttribute<XmlArrayAttribute>()?.ElementName
                                  ?? field.Name;
                if (string.IsNullOrWhiteSpace(elementName))
                    continue;

                var childElement = element.Element(xmlNamespace + elementName);
                if (childElement == null)
                    return true;

                if (field.GetCustomAttribute<XmlArrayAttribute>() != null || !ShouldRecurseInto(field.FieldType))
                    continue;

                var childReference = reference == null ? null : field.GetValue(reference);
                if (childReference != null && HasMissingSerializedMembers(field.FieldType, childElement, childReference))
                    return true;
            }

            return false;
        }

        private static bool ShouldRecurseInto(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (underlyingType.IsPrimitive || underlyingType.IsEnum)
                return false;

            if (underlyingType == typeof(string)
                || underlyingType == typeof(decimal)
                || underlyingType == typeof(DateTime)
                || underlyingType == typeof(DateTimeOffset)
                || underlyingType == typeof(TimeSpan)
                || underlyingType == typeof(Guid))
                return false;

            return !typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType);
        }

        private static bool ShouldSerializeMember(Type type, string memberName, object? reference)
        {
            var method = type.GetMethod($"ShouldSerialize{memberName}",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);
            if (method == null)
                return true;

            return method.Invoke(reference, null) as bool? ?? true;
        }
    }
}
