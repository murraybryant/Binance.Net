﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Converters;
using Binance.Net.Enums;
using Binance.Net.Interfaces.SubClients;
using Binance.Net.Objects.Spot.WalletData;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;

namespace Binance.Net.SubClients
{
    /// <summary>
    /// Withdraw/Deposit endpoints
    /// </summary>
    public class BinanceClientWithdrawDeposit : IBinanceClientWithdrawDeposit
    {
        private const string assetDetailsEndpoint = "asset/assetDetail";
        private const string withdrawEndpoint = "capital/withdraw/apply";
        private const string withdrawHistoryEndpoint = "capital/withdraw/history";
        private const string depositHistoryEndpoint = "capital/deposit/hisrec";
        private const string depositAddressEndpoint = "capital/deposit/address";

        private readonly BinanceClient _baseClient;

        internal BinanceClientWithdrawDeposit(BinanceClient baseClient)
        {
            _baseClient = baseClient;
        }

        #region asset details
        /// <summary>
        /// Gets the withdraw/deposit details for an asset
        /// </summary>
        /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Asset detail</returns>
        public async Task<WebCallResult<Dictionary<string, BinanceAssetDetails>>> GetAssetDetailsAsync(int? receiveWindow = null, CancellationToken ct = default)
        {
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<Dictionary<string, BinanceAssetDetails>>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "timestamp", _baseClient.GetTimestamp() }
            };
            parameters.AddOptionalParameter("recvWindow", receiveWindow?.ToString(CultureInfo.InvariantCulture) ?? _baseClient.DefaultReceiveWindow.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var result = await _baseClient.SendRequestInternal<Dictionary<string, BinanceAssetDetails>>(_baseClient.GetUrlSpot(assetDetailsEndpoint, "sapi", "1"), HttpMethod.Get, ct, parameters, true).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region Withdraw
        /// <summary>
        /// Withdraw assets from Binance to an address
        /// </summary>
        /// <param name="asset">The asset to withdraw</param>
        /// <param name="address">The address to send the funds to</param>
        /// <param name="addressTag">Secondary address identifier for coins like XRP,XMR etc.</param>
        /// <param name="withdrawOrderId">Custom client order id</param>
        /// <param name="amount">The amount to withdraw</param>
        /// <param name="transactionFeeFlag">When making internal transfer, true for returning the fee to the destination account; false for returning the fee back to the departure account. Default false.</param>
        /// <param name="network">The network to use</param>
        /// <param name="name">Description of the address</param>
        /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Withdrawal confirmation</returns>
        public async Task<WebCallResult<BinanceWithdrawalPlaced>> WithdrawAsync(string asset, string address, decimal amount, string? withdrawOrderId = null, string? network = null, string? addressTag = null, string? name = null, bool? transactionFeeFlag = null, int? receiveWindow = null, CancellationToken ct = default)
        {
            asset.ValidateNotNull(nameof(asset));
            address.ValidateNotNull(nameof(address));

            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<BinanceWithdrawalPlaced>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "coin", asset },
                { "address", address },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "timestamp", _baseClient.GetTimestamp() }
            };
            parameters.AddOptionalParameter("name", name);
            parameters.AddOptionalParameter("withdrawOrderId", withdrawOrderId);
            parameters.AddOptionalParameter("network", network);
            parameters.AddOptionalParameter("transactionFeeFlag", transactionFeeFlag);
            parameters.AddOptionalParameter("addressTag", addressTag);
            parameters.AddOptionalParameter("recvWindow", receiveWindow?.ToString(CultureInfo.InvariantCulture) ?? _baseClient.DefaultReceiveWindow.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            var result = await _baseClient.SendRequestInternal<BinanceWithdrawalPlaced>(_baseClient.GetUrlSpot(withdrawEndpoint, "sapi", "1"), HttpMethod.Post, ct, parameters, true, true, PostParameters.InUri).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Withdraw History
        /// <summary>
        /// Gets the withdrawal history
        /// </summary>
        /// <param name="asset">Filter by asset</param>
        /// <param name="withdrawOrderId">Filter by withdraw order id</param>
        /// <param name="status">Filter by status</param>
        /// <param name="startTime">Filter start time from</param>
        /// <param name="endTime">Filter end time till</param>
        /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
        /// <param name="limit">Add limit. Default: 1000, Max: 1000</param>
        /// <param name="offset">Add offset</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of withdrawals</returns>
        public async Task<WebCallResult<IEnumerable<BinanceWithdrawal>>> GetWithdrawalHistoryAsync(string? asset = null, string? withdrawOrderId = null, WithdrawalStatus? status = null, DateTime? startTime = null, DateTime? endTime = null, int? receiveWindow = null, int? limit = null, int? offset = null, CancellationToken ct = default)
        {
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<IEnumerable<BinanceWithdrawal>>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "timestamp", _baseClient.GetTimestamp() }
            };

            parameters.AddOptionalParameter("coin", asset);
            parameters.AddOptionalParameter("withdrawOrderId", withdrawOrderId);
            parameters.AddOptionalParameter("status", status != null ? JsonConvert.SerializeObject(status, new WithdrawalStatusConverter(false)) : null);
            parameters.AddOptionalParameter("startTime", startTime != null ? BinanceClient.ToUnixTimestamp(startTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("endTime", endTime != null ? BinanceClient.ToUnixTimestamp(endTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("recvWindow", receiveWindow?.ToString(CultureInfo.InvariantCulture) ?? _baseClient.DefaultReceiveWindow.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("limit", limit);
            parameters.AddOptionalParameter("offset", offset);

            var result = await _baseClient.SendRequestInternal<IEnumerable<BinanceWithdrawal>>(_baseClient.GetUrlSpot(withdrawHistoryEndpoint, "sapi", "1"), HttpMethod.Get, ct, parameters, true).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Deposit history        
        /// <summary>
        /// Gets the deposit history
        /// </summary>
        /// <param name="coin">Filter by asset</param>
        /// <param name="status">Filter by status</param>
        /// <param name="limit">Amount of results</param>
        /// <param name="offset">Offset the results</param>
        /// <param name="startTime">Filter start time from</param>
        /// <param name="endTime">Filter end time till</param>
        /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of deposits</returns>
        public async Task<WebCallResult<IEnumerable<BinanceDeposit>>> GetDepositHistoryAsync(string? coin = null, DepositStatus? status = null, DateTime? startTime = null, DateTime? endTime = null, int? offset = null, int? limit = null, int? receiveWindow = null, CancellationToken ct = default)
        {
            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<IEnumerable<BinanceDeposit>>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "timestamp", _baseClient.GetTimestamp() }
            };
            parameters.AddOptionalParameter("coin", coin);
            parameters.AddOptionalParameter("offset", offset?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("limit", limit?.ToString(CultureInfo.InvariantCulture));
            parameters.AddOptionalParameter("status", status != null ? JsonConvert.SerializeObject(status, new DepositStatusConverter(false)) : null);
            parameters.AddOptionalParameter("startTime", startTime != null ? BinanceClient.ToUnixTimestamp(startTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("endTime", endTime != null ? BinanceClient.ToUnixTimestamp(endTime.Value).ToString(CultureInfo.InvariantCulture) : null);
            parameters.AddOptionalParameter("recvWindow", receiveWindow?.ToString(CultureInfo.InvariantCulture) ?? _baseClient.DefaultReceiveWindow.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            return await _baseClient.SendRequestInternal<IEnumerable<BinanceDeposit>>(
                    _baseClient.GetUrlSpot(depositHistoryEndpoint, "sapi", "1"), HttpMethod.Get, ct, parameters, true)
                .ConfigureAwait(false);
        }
        #endregion

        #region Get Deposit Address

        /// <summary>
        /// Gets the deposit address for an asset
        /// </summary>
        /// <param name="coin">Asset to get address for</param>
        /// <param name="network">Network</param>
        /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Deposit address</returns>
        public async Task<WebCallResult<BinanceDepositAddress>> GetDepositAddressAsync(string coin, string? network = null, int? receiveWindow = null, CancellationToken ct = default)
        {
            coin.ValidateNotNull(nameof(coin));

            var timestampResult = await _baseClient.CheckAutoTimestamp(ct).ConfigureAwait(false);
            if (!timestampResult)
                return new WebCallResult<BinanceDepositAddress>(timestampResult.ResponseStatusCode, timestampResult.ResponseHeaders, null, timestampResult.Error);

            var parameters = new Dictionary<string, object>
            {
                { "coin", coin },
                { "timestamp", _baseClient.GetTimestamp() }
            };
            parameters.AddOptionalParameter("network", network);
            parameters.AddOptionalParameter("recvWindow", receiveWindow?.ToString(CultureInfo.InvariantCulture) ?? _baseClient.DefaultReceiveWindow.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            return await _baseClient.SendRequestInternal<BinanceDepositAddress>(_baseClient.GetUrlSpot(depositAddressEndpoint, "sapi", "1"), HttpMethod.Get, ct, parameters, true).ConfigureAwait(false);
        }

        #endregion
    }
}
