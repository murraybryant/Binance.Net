﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Interfaces.SubClients;
using Binance.Net.Objects.Spot.UserData;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;

namespace Binance.Net.SubClients.Futures
{
    /// <summary>
    /// Futures user stream endpoints
    /// </summary>
    public abstract class BinanceClientFuturesUserStream : IBinanceClientUserStream
    {
        private const string getFuturesListenKeyEndpoint = "listenKey";
        private const string keepFuturesListenKeyAliveEndpoint = "listenKey";
        private const string closeFuturesListenKeyEndpoint = "listenKey";

        /// <summary>
        /// Api path
        /// </summary>
        protected abstract string Api { get; }
        private const string userDataStreamVersion = "1";

        private readonly BinanceClient _baseClient;
        private readonly BinanceClientFutures _futuresClient;

        internal BinanceClientFuturesUserStream(BinanceClient baseClient, BinanceClientFutures futuresClient)
        {
            _baseClient = baseClient;
            _futuresClient = futuresClient;
        }

        #region Start User Data Stream
        /// <summary>
        /// Starts a user stream by requesting a listen key. This listen key can be used in subsequent requests to BinanceSocketClient.Futures.SubscribeToUserDataUpdates. The stream will close after 60 minutes unless a keep alive is send.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Listen key</returns>
        public async Task<WebCallResult<string>> StartUserStreamAsync(CancellationToken ct = default)
        {
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<string>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var result = await _baseClient.SendRequestInternal<BinanceListenKey>(_futuresClient.GetUrl(getFuturesListenKeyEndpoint, Api, userDataStreamVersion), HttpMethod.Post, ct).ConfigureAwait(false);
            return result.As(result.Data?.ListenKey);
        }

        #endregion

        #region Keepalive User Data Stream

        /// <summary>
        /// Sends a keep alive for the current user stream listen key to keep the stream from closing. Stream auto closes after 60 minutes if no keep alive is send. 30 minute interval for keep alive is recommended.
        /// </summary>
        /// <param name="listenKey">The listen key to keep alive</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public async Task<WebCallResult<object>> KeepAliveUserStreamAsync(string listenKey, CancellationToken ct = default)
        {
            listenKey.ValidateNotNull(nameof(listenKey));
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<object>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "listenKey", listenKey }
            };

            return await _baseClient.SendRequestInternal<object>(_futuresClient.GetUrl(keepFuturesListenKeyAliveEndpoint, Api, userDataStreamVersion), HttpMethod.Put, ct, parameters).ConfigureAwait(false);
        }

        #endregion

        #region Close User Data Stream

        /// <summary>
        /// Stops the current user stream
        /// </summary>
        /// <param name="listenKey">The listen key to keep alive</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public async Task<WebCallResult<object>> StopUserStreamAsync(string listenKey, CancellationToken ct = default)
        {
            listenKey.ValidateNotNull(nameof(listenKey));
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<object>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "listenKey", listenKey }
            };

            return await _baseClient.SendRequestInternal<object>(_futuresClient.GetUrl(closeFuturesListenKeyEndpoint, Api, userDataStreamVersion), HttpMethod.Delete, ct, parameters).ConfigureAwait(false);
        }

        #endregion
    }
}
