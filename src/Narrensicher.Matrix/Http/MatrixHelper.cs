﻿using Microsoft.Extensions.Logging;
using Narrensicher.Matrix.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Narrensicher.Matrix.Http;
/// <summary>
/// Low level helper methods for Matrix operations
/// </summary>
public static class MatrixHelper
{
    /// <summary>
    /// Fetches the well-known URI from the server
    /// This is a low level method, it is implicitly called by ConnectAsync
    /// </summary>
    public static async Task<WellKnownUriResponse> FetchWellKnownUriAsync(HttpClientParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return await HttpClientHelper.SendAsync<WellKnownUriResponse>(parameters, "/.well-known/matrix/client").ConfigureAwait(false);
    }

    public static async Task<SpecVersionsResponse> FetchSupportedSpecVersionsAsync(HttpClientParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return await HttpClientHelper.SendAsync<SpecVersionsResponse>(parameters, "/_matrix/client/versions").ConfigureAwait(false);
    }

    public static async Task<AuthFlowsResponse> FetchSupportedAuthFlowsAsync(HttpClientParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return await HttpClientHelper.SendAsync<AuthFlowsResponse>(parameters, "/_matrix/client/v3/login").ConfigureAwait(false);
    }

    public static async Task<LoginResponse> PasswordLoginAsync(HttpClientParameters parameters, string userId, string password, string deviceId)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));
        ArgumentException.ThrowIfNullOrEmpty(password, nameof(password));
        ArgumentException.ThrowIfNullOrEmpty(deviceId, nameof(deviceId));
        var request = new LoginRequest
        {
            Identifier = new Identifier
            {
                Type = "m.id.user",
                User = userId
            },
            Password = password,
            DeviceId = deviceId,
            InitialDeviceDisplayName = "Narrensicher.Matrix Device",
            Type = "m.login.password"
        };
        var content = JsonContent.Create(request);
        return await HttpClientHelper.SendAsync<LoginResponse>(parameters, "/_matrix/client/v3/login", HttpMethod.Post, content).ConfigureAwait(false);
    }

    public static async Task<CapabilitiesResponse> FetchCapabilitiesAsync(HttpClientParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return await HttpClientHelper.SendAsync<CapabilitiesResponse>(parameters, "/_matrix/client/v3/capabilities").ConfigureAwait(false);
    }

    public static async Task<FilterResponse> PostFilterAsync(HttpClientParameters parameters, MatrixId user, Filter filter)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(user);
        if (user.Kind != IdKind.User)
            throw new ArgumentException("User must be a user ID", nameof(user));
        ArgumentNullException.ThrowIfNull(filter);
        var content = JsonContent.Create(filter, options: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        string path = $"/_matrix/client/v3/user/{HttpUtility.UrlEncode(user.Full)}/filter";
        return await HttpClientHelper.SendAsync<FilterResponse>(parameters, path, HttpMethod.Post, content).ConfigureAwait(false);
    }

    public static async Task<PresenceResponse> GetPresenceAsync(HttpClientParameters parameters, MatrixId user)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(user);
        string path = $"/_matrix/client/v3/presence/{HttpUtility.UrlEncode(user.Full)}/status";
        return await HttpClientHelper.SendAsync<PresenceResponse>(parameters, path).ConfigureAwait(false);
    }

    public static async Task PutPresenceAsync(HttpClientParameters parameters, MatrixId userId, PresenceRequest presence)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(presence);

        var content = JsonContent.Create(
            presence, options: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        string path = $"/_matrix/client/v3/presence/{HttpUtility.UrlEncode(userId.Full)}/status";
        await HttpClientHelper.SendAsync(parameters, path, HttpMethod.Put, content).ConfigureAwait(false);
    }

    public static async Task PostReceiptAsync(HttpClientParameters parameters, MatrixId room, MatrixId eventId, string? threadId)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(eventId);
        var receipt = new ReceiptRequest { ThreadId = threadId };
        var content = JsonContent.Create(
            receipt, options: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        string path = $"/_matrix/client/v3/rooms/{HttpUtility.UrlEncode(room.Full)}/receipt/m.read/{HttpUtility.UrlEncode(eventId.Full)}";
        await HttpClientHelper.SendAsync(parameters, path, HttpMethod.Post, content).ConfigureAwait(false);
    }

    public static async Task PostReadMarkersAsync(HttpClientParameters parameters, MatrixId room, MatrixId eventId)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(eventId);
        var receipt = new ReadMarkerRequest { FullyRead = eventId.Full };
        var content = JsonContent.Create(
            receipt, options: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        string path = $"/_matrix/client/v3/rooms/{HttpUtility.UrlEncode(room.Full)}/read_markers";
        await HttpClientHelper.SendAsync(parameters, path, HttpMethod.Post, content).ConfigureAwait(false);
    }



    public static async Task<Filter> GetFilterAsync(HttpClientParameters parameters, MatrixId user, string filterId)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(user);
        if (user.Kind != IdKind.User)
            throw new ArgumentException("User must be a user ID", nameof(user));
        ArgumentException.ThrowIfNullOrEmpty(filterId, nameof(filterId));
        string path = $"/_matrix/client/v3/user/{HttpUtility.UrlEncode(user.Full)}/filter/{HttpUtility.UrlEncode(filterId)}";
        return await HttpClientHelper.SendAsync<Filter>(parameters, path).ConfigureAwait(false);
    }

    internal static async Task<SyncResponse> GetSyncAsync(HttpClientParameters httpClientParameters, SyncParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(httpClientParameters, nameof(httpClientParameters));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

        var path = HttpParameterHelper.AppendParameters("/_matrix/client/v3/sync", parameters.GetAsParameters());

        return await HttpClientHelper.SendAsync<SyncResponse>(httpClientParameters, path, HttpMethod.Get,
            ignoreRateLimit: true).ConfigureAwait(false);
    }


}
