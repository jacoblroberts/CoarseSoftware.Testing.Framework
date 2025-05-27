namespace CoarseSoftware.Testing.Framework.Core.Serializer
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class UnitTestCaseJsonConverter : JsonConverter<UnitTestCase>
    {
        public override UnitTestCase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
            )
        {
            throw new NotSupportedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            UnitTestCase value,
            JsonSerializerOptions options
            )
        {
            //JsonWriterOptions writerOptions = new() { };
            //using MemoryStream stream = new();
            //using Utf8JsonWriter writer = new(stream, writerOptions);

            var configuration = Helpers.GetTestRunnerConfiguration();
            writer.WriteStartObject();

            writer.WriteString(nameof(UnitTestCase.ConceptInterfaceType), value.ConceptInterfaceType.FullName);
            writer.WriteString(nameof(UnitTestCase.Method), value.Method);
            writer.WriteString(nameof(UnitTestCase.Request), getRequestTypeName(configuration, value.Request));
            writer.WriteString(nameof(UnitTestCase.Description), value.Description);
            writer.WriteStartArray(nameof(UnitTestCase.Operations));
            writeOperations(value.Operations, writer, configuration);
            writer.WriteEndArray();
            writer.WriteEndObject();


            //writer.Flush();
            //string json = Encoding.UTF8.GetString(stream.ToArray());

            //if (json == null)
            //{
            //    throw new Exception();
            //}
        }

        private void writeOperations(IEnumerable<UnitTestCase.Operation> operations, Utf8JsonWriter writer, TestRunnerConfiguration configuration)
        {
            
            if (operations != null)
            {
                foreach (var operation in operations)
                {
                    if (operation is UnitTestCase.ServiceOperation serviceOperation)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("OperationKind", "Service");
                        writer.WriteString(nameof(UnitTestCase.ServiceOperation.FacetType), serviceOperation.FacetType.FullName);
                        writer.WriteString(nameof(UnitTestCase.ServiceOperation.MethodName), serviceOperation.MethodName);
                        writer.WriteString(nameof(UnitTestCase.ServiceOperation.ExpectedRequest), getRequestTypeName(configuration, serviceOperation.ExpectedRequest));

                        // this needs attention
                        writer.WriteStartObject(nameof(UnitTestCase.ServiceOperation.Response));
                        writeResponse(serviceOperation.Response, writer, configuration);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    if (operation is UnitTestCase.ExpectedResponseOperation expectedResponseOperation)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("OperationKind", "ExpectedResponse");
                        writer.WriteString(nameof(UnitTestCase.ExpectedResponseOperation.ExpectedResponse), getResponseTypeName(configuration, expectedResponseOperation.ExpectedResponse));
                        writer.WriteEndObject();
                    }
                }
            }
            
        }

        private void writeResponse(UnitTestCase.Response response, Utf8JsonWriter writer, TestRunnerConfiguration configuration)
        {
            if (response is UnitTestCase.Response.MockResponse mockResponse)
            {
                writer.WriteString("ResponseKind", "Mock");
                writer.WriteString(nameof(UnitTestCase.ServiceOperation.Response), getResponseTypeName(configuration, mockResponse.Response));
            }
            if (response is UnitTestCase.Response.ForkingResponse forkingResponse)
            {
                writer.WriteString("ResponseKind", "Fork");
                // left fork
                writer.WriteStartObject(nameof(UnitTestCase.Response.ForkingResponse.LeftFork));
                writer.WriteString(nameof(UnitTestCase.Response.ForkingResponse.LeftForkResponse.Reason), forkingResponse.LeftFork.Reason);
                writer.WriteString(nameof(UnitTestCase.Response.ForkingResponse.LeftForkResponse.MockResponse), getResponseTypeName(configuration, forkingResponse.LeftFork.MockResponse));
                writer.WriteStartArray(nameof(UnitTestCase.Response.ForkingResponse.LeftForkResponse.Operations));
                writeOperations(forkingResponse.LeftFork.Operations, writer, configuration);
                writer.WriteEndArray();
                writer.WriteEndObject();

                // right fork
                writer.WriteStartObject(nameof(UnitTestCase.Response.ForkingResponse.RightFork));
                writer.WriteString(nameof(UnitTestCase.Response.ForkingResponse.RightForkResponse.Reason), forkingResponse.RightFork.Reason);
                writer.WriteStartObject(nameof(UnitTestCase.Response.ForkingResponse.RightForkResponse.Response));
                writeResponse(forkingResponse.RightFork.Response, writer, configuration);
                writer.WriteEndObject();
                writer.WriteStartArray(nameof(UnitTestCase.Response.ForkingResponse.RightForkResponse.Operations));
                writeOperations(forkingResponse.RightFork.Operations, writer, configuration);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }

            private string getRequestTypeName(TestRunnerConfiguration configuration, object request)
        {
            var requestType = request.GetType();
            var isRequestWrapped = configuration.RequestWrapper != null
                    && requestType.IsGenericType
                    && requestType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            var requestData = isRequestWrapped
                 ? requestType.GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(request)
                 : request;
            return requestData.GetType().FullName;
        }

        private string getResponseTypeName(TestRunnerConfiguration configuration, object response)
        {
            if (response == null)
            {
                return "null";
            }
            var responseType = response.GetType();
            if (responseType == null) 
            { 
                return "null"; 
            }
            var isRequestWrapped = configuration.ResponseWrapper != null
                    && responseType.IsGenericType
                    && responseType.GetGenericTypeDefinition() == configuration.ResponseWrapper.OpenWrapperType;
            var responseData = isRequestWrapped
                 ? responseType.GetProperty(configuration.ResponseWrapper.DtoPropertyName).GetValue(response)
                 : response;
            if (responseData == null)
            {
                return "null";
            }
            return responseData.GetType().FullName;
        }
    }
}
