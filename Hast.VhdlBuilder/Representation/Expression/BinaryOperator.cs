using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class BinaryOperator : IVhdlElement
    {
        private readonly string _source;

        public static readonly BinaryOperator Add = new BinaryOperator("+");
        public static readonly BinaryOperator And = new BinaryOperator("and");
        public static readonly BinaryOperator Divide = new BinaryOperator("/");
        public static readonly BinaryOperator Equality = new BinaryOperator("=");
        public static readonly BinaryOperator ExclusiveOr = new BinaryOperator("xor");
        public static readonly BinaryOperator GreaterThan = new BinaryOperator(">");
        public static readonly BinaryOperator GreaterThanOrEqual = new BinaryOperator(">=");
        public static readonly BinaryOperator InEquality = new BinaryOperator("/=");
        public static readonly BinaryOperator LessThan = new BinaryOperator("<");
        public static readonly BinaryOperator LessThanOrEqual = new BinaryOperator("<=");
        public static readonly BinaryOperator Modulus = new BinaryOperator("mod");
        public static readonly BinaryOperator Multiply = new BinaryOperator("*");
        public static readonly BinaryOperator Or = new BinaryOperator("or");
        public static readonly BinaryOperator Remainder = new BinaryOperator("rem");
        public static readonly BinaryOperator ShiftLeftLogical = new BinaryOperator("sll");
        public static readonly BinaryOperator ShiftRightLogical = new BinaryOperator("srl");
        public static readonly BinaryOperator Subtract = new BinaryOperator("-");

        public static readonly JsonConverter JsonConverter = new BinaryOperatorJsonConverter();

        private BinaryOperator(string source) => _source = source;

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) => _source;

        private class BinaryOperatorJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(BinaryOperator);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // Load the JSON for the Result into a JObject
                var jObject = JObject.Load(reader);

                return new BinaryOperator((string)jObject["Source"]);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var token = JToken.FromObject(value);
                if (token.Type != JTokenType.Object)
                {
                    token.WriteTo(writer);
                }
                else
                {
                    var jObject = (JObject)token;
                    jObject.AddFirst(new JProperty("Source", ((BinaryOperator)value)._source));
                    jObject.WriteTo(writer);
                }

            }
        }
    }
}
