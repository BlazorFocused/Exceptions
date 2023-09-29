// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Testing;

namespace MiddlewareSample.Api.Test;

[CollectionDefinition(nameof(MiddlewareSampleTestCollection))]
public class MiddlewareSampleTestCollection : ICollectionFixture<WebApplicationFactory<Program>>
{ }
