﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Service.Sample.Trippin.Models;
using Xunit;

namespace Microsoft.OData.Service.Sample.Tests
{
    public class TrippinInMemoryE2ETest : E2ETestBase<TrippinDataServiceContext>, IClassFixture<TrippinServiceFixture>
    {
        private const string baseUri = "http://localhost:21248/api/Trippin/";

        public TrippinInMemoryE2ETest()
            : base(new Uri(baseUri))
        {
        }

        [Fact]
        public void TestMetadata()
        {
            TestGetPayloadContains("$metadata", "<EntitySet Name=\"People\"");
        }

        [Fact]
        public void TestEntitySet()
        {
            TestGetPayloadContains("People", "FirstName");
        }

        [Fact]
        public void TestImperativeViewEntitySet()
        {
            TestGetPayloadContains("NewComePeople", "FirstName");
        }

        [Fact]
        public void TestCollectionOfPrimitivePropertyAccess()
        {
            TestGetPayloadContains("People('russellwhyte')/Emails",
                "\"@odata.context\":\"http://localhost:21248/");
            TestGetPayloadContains("People('russellwhyte')/Emails",
                "api/Trippin/$metadata#Collection(Edm.String)\"");

            TestGetPayloadContains("People('russellwhyte')/Emails",
                "\"value\":[");
        }

        [Fact]
        public void TestCollectionOfComplexPropertyAccess()
        {
            var reqStr = "People('russellwhyte')/AddressInfo";
            TestGetPayloadContains(reqStr,
                "\"@odata.context\":\"http://localhost:21248/");
            TestGetPayloadContains(reqStr,
                "api/Trippin/$metadata#Collection(" +
                "Microsoft.OData.Service.Sample.TrippinInMemory.Models.Location)\"");
        }

        [Fact]
        public void TestCollectionOfEnumPropertyAccess()
        {
            var reqStr = "People('russellwhyte')/Features";
            TestGetPayloadContains(reqStr,
                "\"@odata.context\":\"http://localhost:21248/");
            TestGetPayloadContains(reqStr,
                "$metadata#Collection(" +
                "Microsoft.OData.Service.Sample.TrippinInMemory.Models.Feature)\"");
        }

        [Fact]
        public void TestEnumPropertyAccess()
        {
            var reqStr = "People('russellwhyte')/FavoriteFeature";
            TestGetPayloadContains(reqStr,
                "\"@odata.context\":\"http://localhost:21248/");
            TestGetPayloadContains(reqStr,
                "$metadata#" + reqStr);
        }

        [Fact]
        public void TestRawValuedEnumPropertyAccess()
        {
            TestGetPayloadIs("People('russellwhyte')/FavoriteFeature/$value", "Feature1");
        }

        [Fact]
        public void TestCountCollectionOfStructuralProperty()
        {
            TestGetPayloadIs("People('russellwhyte')/Emails/$count", "2");
            TestGetPayloadIs("People('russellwhyte')/AddressInfo/$count", "1");
            TestGetPayloadIs("People('russellwhyte')/Features/$count", "2");
        }

        [Fact]
        public void TestAutoExpandedNavigationProperty()
        {
            TestGetPayloadContains("People", "\"Friends\":[");
        }

        [Theory]
        // Single primitive property with null value
        [InlineData("/People('willieashmore')/MiddleName", 204)]
        // Single primitive property $value with null value
        [InlineData("/People('willieashmore')/MiddleName/$value", 204)]
        // Collection of primitive property with empty value
        [InlineData("/People('willieashmore')/Emails", 200)]
        // Collection of primitive property $value with null value, should throw exception
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People('willieashmore')/Emails/$value", 500)]
        // Collection of primitive property with null collection
        [InlineData("/People('clydeguess')/Emails", 200)]
        // single complex property with null value
        [InlineData("/People('willieashmore')/HomeAddress", 204)]
        // single complex property's propery and complex property has null value
        [InlineData("/People('willieashmore')/HomeAddress/Address", 404)]
        // single complex property's property with null value
        [InlineData("/People('clydeguess')/HomeAddress/Address", 204)]
        // collection of complex property with empty collection value
        [InlineData("/People('willieashmore')/AddressInfo", 200)]
        // collection of complex property's propery and collection of complex property has null value
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People(5)/AddressInfo/Address", 500)]
        // Collection of complex property with null collection
        [InlineData("/People('clydeguess')/AddressInfo", 200)]
        // single navigation property with null value
        [InlineData("/People('willieashmore')/BestFriend", 204)]
        // single navigation property's propery and navigation property has null value
        [InlineData("/People('willieashmore')/BestFriend/MiddleName", 404)]
        // single navigation property's property with null value
        [InlineData("/People('russellwhyte')/BestFriend/MiddleName", 204)]
        // collection of navigation property with empty collection value
        [InlineData("/People('willieashmore')/Friends", 200)]
        // collection of navigation property with null collection value
        [InlineData("/People('clydeguess')/Trips", 200)]
        // collection of navigation property's property and navigation property has null value
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People('willieashmore')/Friends/MiddleName", 500)]
        public void QueryPropertyWithNullValueStatusCode(string url, int expectedCode)
        {
            TestGetStatusCodeIs(url, expectedCode);
        }

        [Theory]
        // Single primitive property
        [InlineData("/People('NoneExist')/MiddleName", 404)]
        // Single primitive property $value
        [InlineData("/People('NoneExist')/MiddleName/$value", 404)]
        // Collection of primitive property
        [InlineData("/People('NoneExist')/Emails", 404)]
        // Collection of primitive property $value
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People('NoneExist')/Emails/$value", 500)]
        // single complex property
        [InlineData("/People('NoneExist')/HomeAddress", 404)]
        // single complex property's property
        [InlineData("/People('NoneExist')/HomeAddress/Address", 404)]
        // collection of complex property
        [InlineData("/People('NoneExist')/AddressInfo", 404)]
        // collection of complex property's propery
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People('NoneExist')/Locations/Address", 500)]
        // single navigation property
        [InlineData("/People('NoneExist')/BestFriend", 404)]
        // single navigation property's propery
        [InlineData("/People('NoneExist')/BestFriend/MiddleName", 404)]
        // collection of navigation property
        [InlineData("/People('NoneExist')/Friends", 404)]
        // collection of navigation property's property
        // TODO should be bad request 400 as this is not allowed, 404 is returned by WebApi Route Match method. 500 is returned actually.
        [InlineData("/People('NoneExist')/Friends/MiddleName", 500)]
        public void QueryPropertyWithNonExistEntity(string url, int expectedCode)
        {
            TestGetStatusCodeIs(url, expectedCode);
        }
    }
}
