﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     //
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nop.Services.EuropaCheckVatService
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat", ConfigurationName="EuropaCheckVatService.checkVatPortType")]
    public interface checkVatPortType
    {
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<EuropaCheckVatService.checkVatResponse> checkVatAsync(EuropaCheckVatService.checkVatRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<EuropaCheckVatService.checkVatApproxResponse> checkVatApproxAsync(EuropaCheckVatService.checkVatApproxRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkVat", WrapperNamespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", IsWrapped=true)]
    public partial class checkVatRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=0)]
        public string countryCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=1)]
        public string vatNumber;
        
        public checkVatRequest()
        {
        }
        
        public checkVatRequest(string countryCode, string vatNumber)
        {
            countryCode = countryCode;
            vatNumber = vatNumber;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkVatResponse", WrapperNamespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", IsWrapped=true)]
    public partial class checkVatResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=0)]
        public string countryCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=1)]
        public string vatNumber;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=2)]
        [System.Xml.Serialization.XmlElementAttribute(DataType="date")]
        public System.DateTime requestDate;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=3)]
        public bool valid;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=4)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string name;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=5)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string address;
        
        public checkVatResponse()
        {
        }
        
        public checkVatResponse(string countryCode, string vatNumber, System.DateTime requestDate, bool valid, string name, string address)
        {
            countryCode = countryCode;
            vatNumber = vatNumber;
            requestDate = requestDate;
            valid = valid;
            name = name;
            address = address;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types")]
    public enum matchCode
    {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Item3,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkVatApprox", WrapperNamespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", IsWrapped=true)]
    public partial class checkVatApproxRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=0)]
        public string countryCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=1)]
        public string vatNumber;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=2)]
        public string traderName;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=3)]
        public string traderCompanyType;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=4)]
        public string traderStreet;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=5)]
        public string traderPostcode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=6)]
        public string traderCity;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=7)]
        public string requesterCountryCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=8)]
        public string requesterVatNumber;
        
        public checkVatApproxRequest()
        {
        }
        
        public checkVatApproxRequest(string countryCode, string vatNumber, string traderName, string traderCompanyType, string traderStreet, string traderPostcode, string traderCity, string requesterCountryCode, string requesterVatNumber)
        {
            countryCode = countryCode;
            vatNumber = vatNumber;
            traderName = traderName;
            traderCompanyType = traderCompanyType;
            traderStreet = traderStreet;
            traderPostcode = traderPostcode;
            traderCity = traderCity;
            requesterCountryCode = requesterCountryCode;
            requesterVatNumber = requesterVatNumber;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkVatApproxResponse", WrapperNamespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", IsWrapped=true)]
    public partial class checkVatApproxResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=0)]
        public string countryCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=1)]
        public string vatNumber;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=2)]
        [System.Xml.Serialization.XmlElementAttribute(DataType="date")]
        public System.DateTime requestDate;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=3)]
        public bool valid;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=4)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string traderName;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=5)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string traderCompanyType;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=6)]
        public string traderAddress;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=7)]
        public string traderStreet;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=8)]
        public string traderPostcode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=9)]
        public string traderCity;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=10)]
        public EuropaCheckVatService.matchCode traderNameMatch;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=11)]
        public EuropaCheckVatService.matchCode traderCompanyTypeMatch;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=12)]
        public EuropaCheckVatService.matchCode traderStreetMatch;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=13)]
        public EuropaCheckVatService.matchCode traderPostcodeMatch;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=14)]
        public EuropaCheckVatService.matchCode traderCityMatch;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ec.europa.eu:taxud:vies:services:checkVat:types", Order=15)]
        public string requestIdentifier;
        
        public checkVatApproxResponse()
        {
        }
        
        public checkVatApproxResponse(
                    string countryCode, 
                    string vatNumber, 
                    System.DateTime requestDate, 
                    bool valid, 
                    string traderName, 
                    string traderCompanyType, 
                    string traderAddress, 
                    string traderStreet, 
                    string traderPostcode, 
                    string traderCity, 
                    EuropaCheckVatService.matchCode traderNameMatch, 
                    EuropaCheckVatService.matchCode traderCompanyTypeMatch, 
                    EuropaCheckVatService.matchCode traderStreetMatch, 
                    EuropaCheckVatService.matchCode traderPostcodeMatch, 
                    EuropaCheckVatService.matchCode traderCityMatch, 
                    string requestIdentifier)
        {
            countryCode = countryCode;
            vatNumber = vatNumber;
            requestDate = requestDate;
            valid = valid;
            traderName = traderName;
            traderCompanyType = traderCompanyType;
            traderAddress = traderAddress;
            traderStreet = traderStreet;
            traderPostcode = traderPostcode;
            traderCity = traderCity;
            traderNameMatch = traderNameMatch;
            traderCompanyTypeMatch = traderCompanyTypeMatch;
            traderStreetMatch = traderStreetMatch;
            traderPostcodeMatch = traderPostcodeMatch;
            traderCityMatch = traderCityMatch;
            requestIdentifier = requestIdentifier;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    public interface checkVatPortTypeChannel : EuropaCheckVatService.checkVatPortType, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("dotnet-svcutil", "0.5.0.0")]
    public partial class checkVatPortTypeClient : System.ServiceModel.ClientBase<EuropaCheckVatService.checkVatPortType>, EuropaCheckVatService.checkVatPortType
    {
        
    /// <summary>
    /// Implement this partial method to configure the service endpoint.
    /// </summary>
    /// <param name="serviceEndpoint">The endpoint to configure</param>
    /// <param name="clientCredentials">The client credentials</param>
    static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public checkVatPortTypeClient() : 
                base(checkVatPortTypeClient.GetDefaultBinding(), checkVatPortTypeClient.GetDefaultEndpointAddress())
        {
            Endpoint.Name = EndpointConfiguration.checkVatPort.ToString();
            ConfigureEndpoint(Endpoint, ClientCredentials);
        }
        
        public checkVatPortTypeClient(EndpointConfiguration endpointConfiguration) : 
                base(checkVatPortTypeClient.GetBindingForEndpoint(endpointConfiguration), checkVatPortTypeClient.GetEndpointAddress(endpointConfiguration))
        {
            Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(Endpoint, ClientCredentials);
        }
        
        public checkVatPortTypeClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(checkVatPortTypeClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(Endpoint, ClientCredentials);
        }
        
        public checkVatPortTypeClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(checkVatPortTypeClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(Endpoint, ClientCredentials);
        }
        
        public checkVatPortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<EuropaCheckVatService.checkVatResponse> checkVatAsync(EuropaCheckVatService.checkVatRequest request)
        {
            return base.Channel.checkVatAsync(request);
        }
        
        public System.Threading.Tasks.Task<EuropaCheckVatService.checkVatApproxResponse> checkVatApproxAsync(EuropaCheckVatService.checkVatApproxRequest request)
        {
            return base.Channel.checkVatApproxAsync(request);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.checkVatPort))
            {
                var result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException($"Could not find endpoint with name \'{endpointConfiguration}\'.");
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.checkVatPort))
            {
                return new System.ServiceModel.EndpointAddress("http://ec.europa.eu/taxation_customs/vies/services/checkVatService");
            }
            throw new System.InvalidOperationException($"Could not find endpoint with name \'{endpointConfiguration}\'.");
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return checkVatPortTypeClient.GetBindingForEndpoint(EndpointConfiguration.checkVatPort);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return checkVatPortTypeClient.GetEndpointAddress(EndpointConfiguration.checkVatPort);
        }
        
        public enum EndpointConfiguration
        {
            
            checkVatPort,
        }
    }
}
