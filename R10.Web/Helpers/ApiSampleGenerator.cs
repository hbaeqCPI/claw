using Newtonsoft.Json;
using R10.Web.Api.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace R10.Web.Helpers
{
    public class ApiSampleGenerator
    {
        /// <summary>
        /// Gets the objects that are serialized as samples by the supported formatters.
        /// </summary>
        public static IDictionary<Type, object> SampleObjects { get; internal set; }

        public static IDictionary<MediaTypeHeaderValue, object> GetSample(object returnType)
        {
            SampleObjects = new Dictionary<Type, object>();

            //Add types: JSON and XML
            Collection<MediaTypeFormatter> formatters = new Collection<MediaTypeFormatter>();
            formatters.Add(new JsonMediaTypeFormatter());
            formatters.Add(new XmlMediaTypeFormatter());

            Type type = returnType.GetType(); 

            var samples = new Dictionary<MediaTypeHeaderValue, object>();
            
            if (type != null)
            {
                object sampleObject = GetSampleObject(type);
                foreach (var formatter in formatters)
                {
                    foreach (MediaTypeHeaderValue mediaType in formatter.SupportedMediaTypes)
                    {
                        if (!samples.ContainsKey(mediaType))
                        {                            
                            object sample = null;
                            //Try generate sample using formatter and sample object
                            if (sampleObject != null)
                            {
                                sample = WriteSampleObjectUsingFormatter(formatter, sampleObject, type, mediaType);
                            }
                            samples.Add(mediaType, WrapSampleIfString(sample));
                        }
                    }
                }
            }
            return samples;
        }

        public static object WriteSampleObjectUsingFormatter(MediaTypeFormatter formatter, object value, Type type, MediaTypeHeaderValue mediaType)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            object sample = String.Empty;
            MemoryStream ms = null;
            HttpContent content = null;
            try
            {
                if (formatter.CanWriteType(type))
                {
                    ms = new MemoryStream();
                    content = new ObjectContent(type, value, formatter, mediaType);
                    formatter.WriteToStreamAsync(type, value, ms, content, null).Wait();
                    ms.Position = 0;
                    StreamReader reader = new StreamReader(ms);
                    string serializedSampleString = reader.ReadToEnd();
                    if (mediaType.MediaType.ToUpperInvariant().Contains("XML"))
                    {
                        serializedSampleString = TryFormatXml(serializedSampleString);
                    }
                    else if (mediaType.MediaType.ToUpperInvariant().Contains("JSON"))
                    {
                        serializedSampleString = TryFormatJson(serializedSampleString);
                    }

                    sample = new TextSample(serializedSampleString);
                }
                else
                {
                    sample = new InvalidSample(String.Format(
                        CultureInfo.CurrentCulture,
                        "Failed to generate the sample for media type '{0}'. Cannot use formatter '{1}' to write type '{2}'.",
                        mediaType,
                        formatter.GetType().Name,
                        type.Name));
                }
            }
            catch (Exception e)
            {
                sample = new InvalidSample(String.Format(
                    CultureInfo.CurrentCulture,
                    "An exception has occurred while using the formatter '{0}' to generate sample for media type '{1}'. Exception message: {2}",
                    formatter.GetType().Name,
                    mediaType.MediaType,
                    e.Message));
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                }
                if (content != null)
                {
                    content.Dispose();
                }
            }

            return sample;
        }

        private static string TryFormatJson(string str)
        {
            try
            {
                object parsedJson = JsonConvert.DeserializeObject(str);
                return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                // can't parse JSON, return the original string
                return str;
            }
        }

        private static string TryFormatXml(string str)
        {
            try
            {
                XDocument xml = XDocument.Parse(str);
                return xml.ToString();
            }
            catch
            {
                // can't parse XML, return the original string
                return str;
            }
        }        

        /// <summary>
        /// Gets the sample object that will be serialized by the formatters. 
        /// First, it will look at the <see cref="SampleObjects"/>. If no sample object is found, it will try to create one using <see cref="ObjectGenerator"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The sample object.</returns>
        public static object GetSampleObject(Type type)
        {
            object sampleObject;

            if (!SampleObjects.TryGetValue(type, out sampleObject))
            {
                // Try create a default sample object
                ObjectGenerator objectGenerator = new ObjectGenerator();
                sampleObject = objectGenerator.GenerateObject(type);
            }

            return sampleObject;
        }

        private static object WrapSampleIfString(object sample)
        {
            string stringSample = sample as string;
            if (stringSample != null)
            {
                return new TextSample(stringSample);
            }

            return sample;
        }

        public static string GetResponseSchema(object returnType)
        {
            //Get xml sample response
            var sampleResponses = GetSample(returnType);
            var temp = sampleResponses.Where(s => s.Key.ToString() == "application/xml").Select(s => s.Value).FirstOrDefault();

            //Create xsd schema
            var reader = XmlReader.Create(new StringReader(temp.ToString()));
            var schemaInference = new XmlSchemaInference();
            var schemaSet = schemaInference.InferSchema(reader);
            var schema = schemaSet.Schemas().OfType<XmlSchema>().FirstOrDefault();
            var schemaData = "";

            using (var stringWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    schema.Write(writer);
                }

                schemaData = stringWriter.ToString();
            }

            return schemaData;
        }
    }

    public class TextSample
    {
        public TextSample(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Text = text;
        }

        public string Text { get; private set; }

        public override bool Equals(object obj)
        {
            TextSample other = obj as TextSample;
            return other != null && Text == other.Text;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class InvalidSample
    {
        public InvalidSample(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage");
            }
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; private set; }

        public override bool Equals(object obj)
        {
            InvalidSample other = obj as InvalidSample;
            return other != null && ErrorMessage == other.ErrorMessage;
        }

        public override int GetHashCode()
        {
            return ErrorMessage.GetHashCode();
        }

        public override string ToString()
        {
            return ErrorMessage;
        }
    }


}